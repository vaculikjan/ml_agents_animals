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
        private float _FleeSpeed = 50f;
        [SerializeField]
        private float _FleeEnergyMultiplier = 1.5f;
        [SerializeField]
        private BufferSensorComponent _ThreatSensor;
        
        [Header("IEdible Variables")]
        [SerializeField]
        private float _TimeToEat;
        [SerializeField]
        private float _FoodValue = 1f;
        
        private int _fixedUpdateCounter;
        private bool Resting => CurrentState?.StateID == AnimalState.Sleep;
        private bool Fleeing => CurrentState?.StateID == AnimalState.Flee;

        private AttributeAggregate _attributeAggregate;
        
        private IThreat<Wolf>[] _threats;
        
        private StatsRecorder _statsRecorder;
        
        public float TimeToEat => _TimeToEat;
        public event FoodEventHandler FoodDepleted = delegate {  };
        
        private void HandleHunger()
        {
            if (_fixedUpdateCounter % 50 == 0)
            {
                _Hunger.Value += _HungerPerSecond * (Resting ? 0.3f : 1f);
            }

            if (!(_Hunger.Value >= _Hunger.MaxValue)) return;
            
            
            //EndEpisode();
            OnDeath(DeathType.Starvation);
        }

        private void HandleEnergy()
        {
            if (_fixedUpdateCounter % 50 == 0)
            {
                _Energy.Value += _EnergyPerSecond * (Fleeing ? _FleeEnergyMultiplier : 1f);
            }
            

            if (!(_Energy.Value <= _Energy.MinValue)) return;
            
            /*
            SetReward(-1000f);
            EndEpisode();
            */
        }
        
        private void HandleCuriosity()
        {
            if (CurrentState?.StateID == AnimalState.Wander)
            {
                _Curiosity.Value += _CuriosityDecayPerSecond * Time.fixedDeltaTime;
                return;
            }
            
            if (_fixedUpdateCounter % 50 != 0) return;
            {
                _Curiosity.Value += _CuriosityPerSecond;
            }
        }
        
        private void HandleThreatDetection()
        {
            if (_fixedUpdateCounter % 25 != 0) return;
            DetectThreats();
        }
        
        protected override void OnFoodDepleted(IEdible food)
        {
            FoodList.Remove(food as IDeerEdible);
            if (AvailableFood.Contains(food))
            {
                AvailableFood[Array.IndexOf(AvailableFood, food as IDeerEdible)] = null;
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

        private void DetectThreats()
        {
            var foundColliders = Physics.OverlapSphere(transform.position, _ThreatDetectionRadius);
            
            var foundThreats = foundColliders.Select(threat => threat.GetComponent<IThreat<Wolf>>()).Where(threatComponent => threatComponent != null).ToList();

            foreach (var currentThreat in _threats)
            {
                if (foundThreats.Contains(currentThreat)) continue;
                _threats[Array.IndexOf(_threats, currentThreat)] = null;
            }

            foreach (var foundThreat in foundThreats.Where(foundThreat => !_threats.Contains(foundThreat)))
            {
                var availableIndex = Array.IndexOf(_threats, foundThreat);
                if (availableIndex == -1) break;
                _threats[availableIndex] = foundThreat;
                var wolfComponent = foundThreat.GetSelf().GetComponent<Wolf>();
                wolfComponent.Died += OnWolfDied;
            }
        }

        private void OnWolfDied(AAnimal<IWolfEdible> animal, DeathType deathtype)
        {
            _threats[Array.IndexOf(_threats, animal as IThreat<Wolf>)] = null;
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
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle}, ActionMap = new List<int> {8}
            }},
        };
    #endregion
        
    #region AgentOverrides
            
        public override void Initialize()
        {
            base.Initialize();
            _statsRecorder = Academy.Instance.StatsRecorder;
            
            _attributeAggregate = new AttributeAggregate(new List<AnimalAttribute> {_Hunger});
            
            AvailableFood = new IDeerEdible[StateParams[AnimalState.Seek].ActionMap.Count];
            _threats = new IThreat<Wolf>[StateParams[AnimalState.Flee].ActionMap.Count];
            
            _fixedUpdateCounter = 0;
            
            _HungerPerSecond = EnvironmentController.Instance.EnvironmentConfig.DeerHungerPerSecond;
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
            // sensor.AddObservation(_Energy.Value);
            // sensor.AddObservation(_Curiosity.Value);
            sensor.AddObservation((int) CurrentState.StateID);

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
                if (threat == null) continue;
                var threatPosition = threat.GetSelf().transform.localPosition;
                var distanceToThreat = Vector3.Distance(transform.localPosition, threatPosition);
                var threatObservation = new float[]
                {
                    distanceToThreat
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
                    SetState(new WanderState(this, _MovementSpeed, _RotationSpeed, EnvironmentController.Instance.ArenaBounds));
                    break;
                case AnimalState.Seek:
                    SetState(new SeekState(this, _MovementSpeed, _RotationSpeed, AvailableFood[actions.DiscreteActions[0] - StateParams[AnimalState.Seek].ActionMap.First()].GetSelf().transform.position));
                    break;
                case AnimalState.Eat:
                    SetState(new EatState(this, NearestFood));
                    break;
                case AnimalState.Sleep:
                    SetState(new SleepState(this, _TimeToSleep));
                    break;
                case AnimalState.Flee:
                    SetState(new FleeState(this, _FleeSpeed, _RotationSpeed, _threats[actions.DiscreteActions[0] - StateParams[AnimalState.Flee].ActionMap.First()].GetSelf().transform, _ThreatDetectionRadius, EnvironmentController.Instance.ArenaBounds));
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            for (var i = 1; i < MaxDiscreteStates; i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            
            if (CurrentState == null || CurrentState.StateID is AnimalState.Sleep)
            {
                return;
            }
            
            StateParams[CurrentState.StateID].ValidTransitions.ForEach(x => SetStateMask(ref actionMask, x));
            
            if (_Energy.Value > 0.5f)
            {
                MaskState(AnimalState.Sleep, ref actionMask, 0, false);
            }
            
            if (!CanEatAvailableFood(out NearestFood))
            {
                MaskState(AnimalState.Eat, ref actionMask, 0, false);
            }
            
            for (var i = StateParams[AnimalState.Seek].ActionMap.First(); i <= StateParams[AnimalState.Seek].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            
            if (CurrentState.StateID is not AnimalState.Eat and AnimalState.Seek)
            {
                MaskSeekState(ref actionMask);
            }
         
            for (var i = StateParams[AnimalState.Flee].ActionMap.First(); i <= StateParams[AnimalState.Flee].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            
            if (CurrentState.StateID is AnimalState.Flee || _Energy.Value < 0.3f)
            {
                return;
            }
            
            MaskFleeState(ref actionMask);
        }

        private void MaskFleeState(ref IDiscreteActionMask actionMask)
        {
            for (var i = StateParams[AnimalState.Flee].ActionMap.First(); i <= StateParams[AnimalState.Flee].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, i - StateParams[AnimalState.Flee].ActionMap.First() < _threats.Length && _threats[i - StateParams[AnimalState.Flee].ActionMap.First()] != null);
            }
        }
    #endregion
        
    #region UnityEvents
        private void FixedUpdate()
        {
            // reward distribution

            CalculateClosestFood();
            CurrentState?.Execute();
            
            _fixedUpdateCounter++;
            
            HandleLifeSpan();
            HandleHunger();
            HandleEnergy();
            // HandleCuriosity();
            HandleThreatDetection();
            
            if (_fixedUpdateCounter % 50 != 0) return;
            var reward = _attributeAggregate.CalculateReward();
            AddReward(reward);
            _statsRecorder.Add("Hunger", _Hunger.Value);
            _statsRecorder.Add("Energy", _Energy.Value);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            DrawCircle(transform.position, _ThreatDetectionRadius, Color.red);
        }
    #endregion
    }
}