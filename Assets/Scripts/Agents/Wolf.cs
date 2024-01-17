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

namespace Agents
{
    public class Wolf : AAnimal<IWolfEdible>
    {
        [Header("Wolf variables")]
        [SerializeField]
        private float _AttackRange;
        [SerializeField]
        private float _PursueSpeed;
        
        private int _fixedUpdateCounter;
        private AttributeAggregate _attributeAggregate;
        private IEdible _foodToEat;

        private bool Resting => CurrentState?.StateID == AnimalState.Sleep;

        protected override Dictionary<AnimalState, AnimalStateInfo> StateParams => new()
        {
            {AnimalState.None, new AnimalStateInfo
            {
                ActionMap = new List<int> {0}
            }},
            {AnimalState.Idle, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Wander, AnimalState.Seek, AnimalState.Eat, AnimalState.Sleep, AnimalState.Pursue}, ActionMap = new List<int> {1}
            }},
            {AnimalState.Wander, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Seek, AnimalState.Sleep, AnimalState.Eat, AnimalState.Pursue}, ActionMap = new List<int> {2}
            }},
            {AnimalState.Seek, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Eat, AnimalState.Pursue}, ActionMap = new List<int> {3}
            }},
            {AnimalState.Pursue, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Attack}, ActionMap = new List<int> {4}
            }},
            {AnimalState.Attack, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Eat}, ActionMap = new List<int> {}
            }},
            {AnimalState.Eat, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Wander, AnimalState.Eat}, ActionMap = new List<int> {}
            }},
            {AnimalState.Sleep, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle}, ActionMap = new List<int> {5}
            }},
        };
        
    #region UnityEvents
        private void FixedUpdate()
        {
            // reward distribution
            
            CalculateClosestFood();
            CurrentState?.Execute();
            
            _fixedUpdateCounter++;
            
            HandleHunger();
            //HandleEnergy();
            
            if (_fixedUpdateCounter % 50 != 0) return;
            var reward = _attributeAggregate.CalculateReward();
            AddReward(reward);
        }
        
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            DrawCircle(transform.position, _AttackRange, Color.red);
        }
    #endregion
        
    #region AgentOverrides
        
        public override void Initialize() 
        {
            base.Initialize();
            
            _attributeAggregate = new AttributeAggregate(new List<AnimalAttribute> {_Hunger});
            
            AvailableFood = new IWolfEdible[StateParams[AnimalState.Seek].ActionMap.Count];
            
            _fixedUpdateCounter = 0;
        }
            
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (Input.GetKey(KeyCode.A)) actionsOut.DiscreteActions.Array[0] = 1;
            else if (Input.GetKey(KeyCode.S)) actionsOut.DiscreteActions.Array[0] = 2;
            else if (Input.GetKey(KeyCode.D)) actionsOut.DiscreteActions.Array[0] = 3;
            else if (Input.GetKey(KeyCode.F)) actionsOut.DiscreteActions.Array[0] = 4;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_Hunger.Value);
            sensor.AddObservation(_Energy.Value);
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
            
            switch (state)
            {
                case AnimalState.None:
                    return;
                case AnimalState.Idle: SetState(new IdleState(this));
                    break;
                case AnimalState.Wander: SetState(new WanderState(this, _MovementSpeed, _RotationSpeed, EnvironmentController.Instance.ArenaBounds));
                    break;
                case AnimalState.Seek: SetState(new SeekState(this, _MovementSpeed, _RotationSpeed, AvailableFood[0].GetSelf().transform.position));
                    break;
                case AnimalState.Pursue: SetState(new PursueState(this, _PursueSpeed, _RotationSpeed, AvailableFood[0], 2f, _AttackRange));
                    break;
                case AnimalState.Eat: SetState(new EatState(this, _foodToEat));
                    break;
                case AnimalState.Sleep: SetState(new SleepState(this, _TimeToSleep));
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
            
            if (CurrentState == null || CurrentState.StateID is AnimalState.Sleep || CurrentState.StateID is AnimalState.Attack)
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
            
            for (var i = StateParams[AnimalState.Pursue].ActionMap.First(); i <= StateParams[AnimalState.Pursue].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            
            if (CurrentState.StateID is AnimalState.Eat or AnimalState.Pursue)
            {
                return;
            }
            
            MaskPursueState(ref actionMask);
            
            if (CurrentState.StateID is AnimalState.Seek)
            {
                return;
            }
            
            MaskSeekState(ref actionMask);
        }
        
    #endregion

        private void HandleHunger()
        {
            
            if (_fixedUpdateCounter % 50 == 0)
            {
                _Hunger.Value += _HungerPerSecond * (Resting ? 0.5f : 1f);
            }

            if (!(_Hunger.Value >= _Hunger.MaxValue)) return;
            
            SetReward(-100f);
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
        
        protected override void OnFoodDepleted(IEdible food)
        {
            FoodList.Remove(food as IWolfEdible);
            if (AvailableFood.Contains(food))
            {
                AvailableFood[Array.IndexOf(AvailableFood, food as IWolfEdible)] = null;
            }
        }
        
        private void MaskPursueState(ref IDiscreteActionMask actionMask)
        {
            for (var i = StateParams[AnimalState.Pursue].ActionMap.First(); i <= StateParams[AnimalState.Pursue].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, i - StateParams[AnimalState.Pursue].ActionMap.First() < AvailableFood.Length && AvailableFood[i - StateParams[AnimalState.Pursue].ActionMap.First()] != null);
            }
        }
    }
}
