// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.Linq;
using AgentProperties.Attributes;
using Environment;
using StateMachine.AnimalStates;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
    public class Deer : AAnimal<IDeerEdible>, IWolfEdible
    {
        [Header("Deer Specific")]
        [SerializeField]
        private float _ThreatDetectionRadius = 10f;
        [SerializeField]
        private float _SafetyDistance = 5f;
        [SerializeField]
        private float _ThreatRadiusOffset = 3f;
        [SerializeField]
        private float _FleeAccelerationMultiplier = 1.6f;
        [SerializeField]
        private float _FleeEnergyMultiplier = 1.5f;
        [SerializeField]
        private BufferSensorComponent _ThreatSensor;
        [SerializeField]
        private int _ThreatDetectionInterval = 25;
        
        [Header("IEdible Variables")]
        [SerializeField]
        private float _TimeToEat;
        [SerializeField]
        private float _FoodValue = 1f;
        
        private bool Resting => CurrentState?.StateID == AnimalState.Sleep;
        private bool Fleeing => CurrentState?.StateID == AnimalState.Flee;

        private AttributeAggregate _attributeAggregate;
        
        private IThreat<Wolf>[] _threats;
        
        public float TimeToEat => _TimeToEat;
        public event FoodEventHandler FoodDepleted = delegate {  };
        
        protected override void HandleHunger()
        {
            if (FixedUpdateCounter % 50 == 0)
            {
                _Hunger.Value += _HungerPerSecond * (Resting ? 0.3f : 1f);
            }

            if (!(_Hunger.Value >= _Hunger.MaxValue)) return;
            
            
            OnDeath(DeathType.Starvation);
        }

        protected override void HandleEnergy()
        {
            if (FixedUpdateCounter % 50 == 0)
            {
                _Energy.Value += _EnergyPerSecond * (Fleeing ? _FleeEnergyMultiplier : 1f);
            }
        }
        
        private void HandleCuriosity()
        {
            if (CurrentState?.StateID == AnimalState.Wander)
            {
                _Curiosity.Value += _CuriosityDecayPerSecond * Time.fixedDeltaTime;
                return;
            }
            
            if (FixedUpdateCounter % 50 != 0) return;
            {
                _Curiosity.Value += _CuriosityPerSecond;
            }
        }
        
        private void HandleThreatDetection()
        {
            if (FixedUpdateCounter % _ThreatDetectionInterval != 0) return;
            DetectThreats();
        }
        
        protected override void OnFoodDepleted(IEdible food)
        {
            FoodList.Remove(food as IDeerEdible);
            if (AvailableFood.Contains(food))
            {
                AvailableFood[Array.IndexOf(AvailableFood, food as IDeerEdible)] = null;
            }
            
            if (NearestFood == food)
            {
                NearestFood = null;
            }
        }
        
        public float Eat()
        {
            FoodDepleted(this);
            OnDeath(DeathType.Eaten);
            return _FoodValue;
        }
        
        public new MonoBehaviour GetSelf()
        {
            return this;
        }

        public void GetAttacked()
        {
            // play death animation
        }

        public IAnimal GetAnimal() { return this;}

        private void DetectThreats()
        {
            var foundColliders = Physics.OverlapSphere(transform.position + new Vector3(0,0, _ThreatRadiusOffset), _ThreatDetectionRadius);
            
            var foundThreats = foundColliders.Select(threat => threat.GetComponent<IThreat<Wolf>>()).Where(threatComponent => threatComponent != null).ToList();

            foreach (var currentThreat in _threats)
            {
                if (foundThreats.Contains(currentThreat)) continue;
                _threats[Array.IndexOf(_threats, currentThreat)] = null;
            }

            foreach (var foundThreat in foundThreats.Where(foundThreat => !_threats.Contains(foundThreat)))
            {
                var availableIndex = Array.IndexOf(_threats, null);
                if (availableIndex == -1) break;
                _threats[availableIndex] = foundThreat;
                var wolfComponent = foundThreat.GetSelf().GetComponent<Wolf>();
                wolfComponent.Died += OnWolfDied;
            }
        }

        private void OnWolfDied(AAnimal<IWolfEdible> animal, DeathType deathtype)
        {
            _threats = _threats.Where(x => x != null && !x.Equals(null)).ToArray();
        }

    #region AnimalOverrides
        protected override Dictionary<AnimalState, AnimalStateInfo> StateParams => new()
        {
            {AnimalState.None, new AnimalStateInfo
            {
                ActionMap = new List<int> {0}
            }},
            
            {AnimalState.Idle, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Wander, AnimalState.Seek, AnimalState.Eat, AnimalState.Sleep, AnimalState.Flee}, ActionMap = new List<int> {1}
            }},
            {AnimalState.Wander, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Seek, AnimalState.Sleep, AnimalState.Eat, AnimalState.Flee}, ActionMap = new List<int> {2}
            }},
            {AnimalState.Seek, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Eat, AnimalState.Flee}, ActionMap = new List<int> {3,4,5}
            }},
            {AnimalState.Eat, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Wander, AnimalState.Eat}, ActionMap = new List<int> {6}
            }},
            {AnimalState.Sleep, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle}, ActionMap = new List<int> {7}
            }},
            {AnimalState.Flee, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle}, ActionMap = new List<int> {8, 9, 10}
            }},
        };
    #endregion
        
    #region AgentOverrides
            
        public override void Initialize()
        {
            LoadFromConfig(EnvironmentController.Instance.EnvironmentConfig.DeerConfig);

            base.Initialize();
            
            _attributeAggregate = new AttributeAggregate(new List<AnimalAttribute> {_Hunger, _Energy});
            
            AvailableFood = new IDeerEdible[StateParams[AnimalState.Seek].ActionMap.Count];
            _threats = new IThreat<Wolf>[StateParams[AnimalState.Flee].ActionMap.Count];
            
            FixedUpdateCounter = 0;
        }

        protected override void LoadFromConfig(IAgentConfig config)
        {
            base.LoadFromConfig(config);
            if (config is not DeerConfig deerConfig) return;

            _ThreatDetectionRadius = deerConfig.ThreatDetectionRadius;
            _FleeAccelerationMultiplier = deerConfig.FleeAccelMultiplier;
            _FleeEnergyMultiplier = deerConfig.FleeEnergyMultiplier;
            _ThreatDetectionInterval = deerConfig.ThreatDetectionInterval;
            _ThreatRadiusOffset = deerConfig.ThreatRadiusOffset;
            _SafetyDistance = deerConfig.SafeDistance;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (Input.GetKey(KeyCode.Q)) actionsOut.DiscreteActions.Array[0] = (int) AnimalState.Idle;
            else if (Input.GetKey(KeyCode.W)) actionsOut.DiscreteActions.Array[0] = (int) AnimalState.Wander;
            else if (Input.GetKey(KeyCode.E)) actionsOut.DiscreteActions.Array[0] = 3;
            else if (Input.GetKey(KeyCode.R)) actionsOut.DiscreteActions.Array[0] = 6;
            else if (Input.GetKey(KeyCode.T)) actionsOut.DiscreteActions.Array[0] = (int) AnimalState.Sleep;
            else if (Input.GetKey(KeyCode.Y)) actionsOut.DiscreteActions.Array[0] = 8;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_Hunger.Value);
            sensor.AddObservation(_Energy.Value);
            sensor.AddObservation(_Curiosity.Value);
            sensor.AddObservation(StateMemory.ToList());

            foreach (var food in AvailableFood)
            {
                if (food == null) continue;
                var foodPosition = food.GetSelf().transform.localPosition;
                var distanceToFood = Vector3.Distance(transform.localPosition, foodPosition);
                var foodObservation = new float[]
                {
                    distanceToFood
                };
                _FoodSensor.AppendObservation(foodObservation);
            }
            foreach (var threat in _threats)
            {
                if (threat == null || threat.Equals(null)) continue;
                var self = threat?.GetSelf();
                if (self == null)
                {
                    continue;
                }

                if (self.transform == null)
                {
                    continue;
                }

                var threatPosition = self.transform.localPosition;
                var distanceToThreat = Vector3.Distance(transform.localPosition, threatPosition);
                var threatObservation = new float[]
                {
                    distanceToThreat,
                    (float) threat.GetSelf().GetComponent<Wolf>().CurrentState.StateID
                };
                _ThreatSensor.AppendObservation(threatObservation);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var state = StateParams.Where(x => x.Value.ActionMap.Contains(actions.DiscreteActions[0])).Select(x => x.Key).ToList().First();

            switch(state)
            {
                case AnimalState.None:
                    return;
                case AnimalState.Idle:
                    SetState(new IdleState(this));
                    break;
                case AnimalState.Wander:
                    SetState(new WanderState(this, _RotationSpeed, EnvironmentController.Instance.ArenaBounds));
                    break;
                case AnimalState.Seek:
                    Debug.Log("Deer seeking food");
                    SetState(new SeekState(this, _RotationSpeed, AvailableFood[actions.DiscreteActions[0] - StateParams[AnimalState.Seek].ActionMap.First()].GetSelf().transform.position));
                    break;
                case AnimalState.Eat:
                    SetState(new EatState(this, NearestFood));
                    break;
                case AnimalState.Sleep:
                    SetState(new SleepState(this, _TimeToSleep));
                    break;
                case AnimalState.Flee:
                    if (_threats[actions.DiscreteActions[0] - StateParams[AnimalState.Flee].ActionMap.First()] == null || _threats[actions.DiscreteActions[0] - StateParams[AnimalState.Flee].ActionMap.First()].Equals(null))
                    {
                        return;
                    }
                    SetState(new FleeState(this, _FleeAccelerationMultiplier, _RotationSpeed, _threats[actions.DiscreteActions[0] - StateParams[AnimalState.Flee].ActionMap.First()].GetSelf().transform, _SafetyDistance, EnvironmentController.Instance.ArenaBounds));
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            AddStateToMemory((int) CurrentState.StateID);
        }
        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            for (var i = 1; i < MaxDiscreteStates; i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            
            if (CurrentState == null || CurrentState.StateID is AnimalState.Sleep or AnimalState.Eat)
            {
                return;
            }

            // If the agent is seeking and has enough energy, only available action is fleeing
            if (CurrentState.StateID == AnimalState.Seek)
            {
                TryEnableFleeState(ref actionMask);
                return;
            }
            
            // Check valid transitions from dict
            StateParams[CurrentState.StateID].ValidTransitions.ForEach(x => SetStateMask(ref actionMask, x));
            
            // Disable sleep if energy is high
            if (_Energy.Value > 0.5f)
            {
                MaskState(AnimalState.Sleep, ref actionMask, 0, false);
            }
            
            // Disable eat if there is no food
            if (!CanEatAvailableFood(out NearestFood))
            {
                MaskState(AnimalState.Eat, ref actionMask, 0, false);
            }
            
            // Mask out seek if it was in valid transitions
            for (var i = StateParams[AnimalState.Seek].ActionMap.First(); i <= StateParams[AnimalState.Seek].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            if (CurrentState.StateID != AnimalState.Seek && CurrentState.StateID != AnimalState.Flee)
            {
                TryEnableSeekState(ref actionMask);
            }
            
            // Mask out flee if it was in valid transitions
            for (var i = StateParams[AnimalState.Flee].ActionMap.First(); i <= StateParams[AnimalState.Flee].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            
            if (CurrentState.StateID is AnimalState.Flee)
            {
                return;
            }
            
            TryEnableFleeState(ref actionMask);
        }

        private void TryEnableFleeState(ref IDiscreteActionMask actionMask)
        {
            for (var i = StateParams[AnimalState.Flee].ActionMap.First(); i <= StateParams[AnimalState.Flee].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, i - StateParams[AnimalState.Flee].ActionMap.First() < _threats.Length && _threats[i - StateParams[AnimalState.Flee].ActionMap.First()] != null);
            }
        }
    #endregion
        
    #region UnityEvents
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            HandleThreatDetection();
            
            if (FixedUpdateCounter % 50 != 0) return;
            var reward = _attributeAggregate.CalculateReward();
            AddReward(reward);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            DrawCircle(transform.position + new Vector3(0,0, _ThreatRadiusOffset), _ThreatDetectionRadius, Color.red);
        }
    #endregion
    }
}