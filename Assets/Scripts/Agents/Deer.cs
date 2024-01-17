// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.Linq;
using AgentProperties.Attributes;
using Environment;
using StateMachine.AnimalStates;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agents
{
    public class Deer : AAnimal<IDeerEdible>, IWolfEdible
    {
        [Header("Deer Specific")]
        [SerializeField]
        private float _MaxLifeSpan = 180f;
        [SerializeField]
        private float _MinLifeSpan = 120f;
        
        [Header("IEdible Variables")]
        [SerializeField]
        private float _TimeToEat;
        [SerializeField]
        private float _FoodValue = 1f;
        
        private float _currentLifeSpan;
        private float _timeLiving;

        private int _fixedUpdateCounter;
        private bool Resting => CurrentState?.StateID == AnimalState.Sleep;

        private AttributeAggregate _attributeAggregate;
        
        public float TimeToEat => _TimeToEat;
        public event FoodEventHandler FoodDepleted = delegate {  };
        
        private void HandleHunger()
        {
            if (_fixedUpdateCounter % 50 == 0)
            {
                _Hunger.Value += _HungerPerSecond * (Resting ? 0.5f : 1f);
            }

            if (!(_Hunger.Value >= _Hunger.MaxValue)) return;
            
            SetReward(-1000f);
            EndEpisode();
        }

        private void HandleEnergy()
        {
            if (_fixedUpdateCounter % 50 == 0)
            {
                _Energy.Value += _EnergyPerSecond;
            }
            

            if (!(_Energy.Value <= _Energy.MinValue)) return;
            
            SetReward(-1000f);
            EndEpisode();
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
        
        private void HandleLifeSpan()
        {
            _timeLiving += Time.fixedDeltaTime;
            if (_timeLiving < _currentLifeSpan) return;
            
            EndEpisode();
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

    #region AnimalOverrides
        protected override Dictionary<AnimalState, AnimalStateInfo> StateParams => new()
        {
            {AnimalState.None, new AnimalStateInfo
            {
                ActionMap = new List<int> {0}
            }},
            
            {AnimalState.Idle, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Wander, AnimalState.Seek, AnimalState.Eat, AnimalState.Sleep}, ActionMap = new List<int> {1}
            }},
            {AnimalState.Wander, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Seek, AnimalState.Sleep, AnimalState.Eat}, ActionMap = new List<int> {2}
            }},
            {AnimalState.Seek, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Eat}, ActionMap = new List<int> {3,4,5}
            }},
            {AnimalState.Eat, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Wander, AnimalState.Eat}, ActionMap = new List<int> {6}
            }},
            {AnimalState.Sleep, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle}, ActionMap = new List<int> {7}
            }},
        };
    #endregion
        
    #region AgentOverrides
            
        public override void Initialize()
        {
            base.Initialize();
            
            _attributeAggregate = new AttributeAggregate(new List<AnimalAttribute> {_Hunger});
            
            AvailableFood = new IDeerEdible[StateParams[AnimalState.Seek].ActionMap.Count];
            
            _currentLifeSpan = Random.Range(_MinLifeSpan, _MaxLifeSpan);
            _timeLiving = 0f;
            
            _fixedUpdateCounter = 0;
        }
            
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (Input.GetKey(KeyCode.Q)) actionsOut.DiscreteActions.Array[0] = (int) AnimalState.Idle;
            else if (Input.GetKey(KeyCode.W)) actionsOut.DiscreteActions.Array[0] = (int) AnimalState.Wander;
            else if (Input.GetKey(KeyCode.E)) actionsOut.DiscreteActions.Array[0] = 3;
            else if (Input.GetKey(KeyCode.R)) actionsOut.DiscreteActions.Array[0] = 6;
            else if (Input.GetKey(KeyCode.T)) actionsOut.DiscreteActions.Array[0] = (int) AnimalState.Sleep;
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
            
            if (CurrentState.StateID is AnimalState.Eat or AnimalState.Seek)
            {
                return;
            }
            
            MaskSeekState(ref actionMask);
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
            // HandleEnergy();
            // HandleCuriosity();
            
            if (_fixedUpdateCounter % 50 != 0) return;
            var reward = _attributeAggregate.CalculateReward();
            AddReward(reward);
        }
    #endregion
    }
}