// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.Linq;
using AgentProperties.Attributes;
using Environment;
using StateMachine.AnimalStates;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agents
{
    public class Wolf : AAnimal<IWolfEdible>, IThreat<Wolf>
    {
        [Header("Wolf variables")]
        [SerializeField]
        private float _AttackRange;
        [SerializeField]
        private float _PursueAccelMultiplier;
        [SerializeField]
        private float _PursueEnergyMultiplier = 1.5f;
        [SerializeField]
        private float _ChaseCooldown = 5f;
        
        private AttributeAggregate _attributeAggregate;
        private IEdible _foodToEat;
        private IAnimal _targetFood;

        private bool Resting => CurrentState?.StateID == AnimalState.Sleep;
        private bool Pursuing => CurrentState?.StateID == AnimalState.Pursue;
        
        private float _currentChaseCooldown;

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
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Eat, AnimalState.Pursue}, ActionMap = new List<int> {3,4,5}
            }},
            {AnimalState.Pursue, new AnimalStateInfo
            {
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle, AnimalState.Attack}, ActionMap = new List<int> {6,7,8}
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
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Idle}, ActionMap = new List<int> {9}
            }},
        };
        
        public override void DetectFood()
        {
            var foundColliders = Physics.OverlapSphere(transform.position, _FoodDetectionRadius);
            foreach (var food in foundColliders)
            {
                var foodComponent = food.GetComponent<IWolfEdible>();
                var deerComponent = foodComponent as Deer;
                
                if (foodComponent == null || deerComponent == null) continue;
                if (FoodList.Contains(foodComponent)) continue;
                
                AddReward(DetectFoodReward);
                FoodList.Add(foodComponent);
                
                foodComponent.FoodDepleted += OnFoodDepleted;
                deerComponent.Died += OnFoodDied;
            }
        }

        private void OnFoodDied(AAnimal<IDeerEdible> animal, DeathType deathtype)
        {
            var foodComponent = animal as IWolfEdible;
            if (foodComponent == null) return;
            
            FoodList.Remove(foodComponent);
            
            var index = Array.IndexOf(AvailableFood, foodComponent);
            
            if (index != -1)
            {
                AvailableFood[index] = null;
            }

            if (NearestFood == foodComponent)
            {
                NearestFood = null;
            }
        }

    #region UnityEvents
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_currentChaseCooldown >= 0)
            {
                _currentChaseCooldown -= Time.fixedDeltaTime;
            }
            if (FixedUpdateCounter % 50 != 0) return;
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
            LoadFromConfig(EnvironmentController.Instance.EnvironmentConfig.WolfConfig);

            base.Initialize();
            
            _attributeAggregate = new AttributeAggregate(new List<AnimalAttribute> {_Hunger, _Energy});
            
            AvailableFood = new IWolfEdible[StateParams[AnimalState.Seek].ActionMap.Count];
            
            FixedUpdateCounter = 0;
        }

        protected override void LoadFromConfig(IAgentConfig config)
        {
            base.LoadFromConfig(config);

            if (config is not WolfConfig wolfConfig) return;
            
            _AttackRange = wolfConfig.AttackRange;
            _PursueAccelMultiplier = wolfConfig.PursuitAccelMultiplier;
            _PursueEnergyMultiplier = wolfConfig.PursuitEnergyMultiplier;
            _ChaseCooldown = wolfConfig.ChaseCooldown;
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
            sensor.AddObservation(StateMemory.ToList());

            foreach (var food in AvailableFood)
            {
                if (food == null) continue;
                var foodObject = food.GetSelf();
                if (foodObject == null) continue;
                
                var distanceToFood = Vector3.Distance(transform.localPosition, foodObject.transform.localPosition);
                var foodObservation = new float[]
                {
                    distanceToFood,
                    (float) foodObject.GetComponent<Deer>().CurrentState.StateID
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
                    _targetFood = null;
                    break;
                case AnimalState.Wander: SetState(new WanderState(this, _RotationSpeed, EnvironmentController.Instance.ArenaBounds));
                    _targetFood = null;
                    break;
                case AnimalState.Seek: SetState(new SeekState(this, _RotationSpeed, AvailableFood[actions.DiscreteActions[0] - StateParams[AnimalState.Seek].ActionMap.First()].GetSelf().transform.position));
                    _targetFood = AvailableFood[actions.DiscreteActions[0] - StateParams[AnimalState.Seek].ActionMap.First()].GetAnimal();
                    break;
                case AnimalState.Pursue: SetState(new PursueState(this, _PursueAccelMultiplier, _RotationSpeed, AvailableFood[actions.DiscreteActions[0] - StateParams[AnimalState.Pursue].ActionMap.First()], 2f, _AttackRange));
                    _targetFood = AvailableFood[actions.DiscreteActions[0] - StateParams[AnimalState.Pursue].ActionMap.First()].GetAnimal();
                    break;
                case AnimalState.Eat: SetState(new EatState(this, _foodToEat));
                    _targetFood = null;
                    break;
                case AnimalState.Sleep: SetState(new SleepState(this, _TimeToSleep));
                    _targetFood = null;
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
            
            if (_currentChaseCooldown < 0)
                TryEnablePursueState(ref actionMask);

            if (CurrentState.StateID is AnimalState.Seek)
            {
                return;
            }
            if (_currentChaseCooldown < 0)
                TryEnableSeekState(ref actionMask);
        }
        
    #endregion

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
                _Energy.Value += _EnergyPerSecond * (Pursuing ? _PursueEnergyMultiplier : 1f);
            }
            

            if (!(_Energy.Value <= _Energy.MinValue)) return;
        }
        
        protected override void OnFoodDepleted(IEdible food)             
        {
            FoodList.Remove(food as IWolfEdible);
            
            var index = Array.IndexOf(AvailableFood, food as IWolfEdible);
            
            if (index != -1)
            {
                AvailableFood[index] = null;
            }

            if (NearestFood == food as IWolfEdible)
            {
                NearestFood = null;
            }
        }
        
        private void TryEnablePursueState(ref IDiscreteActionMask actionMask)
        {
            for (var i = StateParams[AnimalState.Pursue].ActionMap.First(); i <= StateParams[AnimalState.Pursue].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, i - StateParams[AnimalState.Pursue].ActionMap.First() < AvailableFood.Length && AvailableFood[i - StateParams[AnimalState.Pursue].ActionMap.First()] != null);
            }
        }

        public bool DetectThreat(out Wolf animal)
        {
            animal = this;
            return CurrentState.StateID switch
            {
                AnimalState.Pursue => Random.Range(0, 100) < 75,
                AnimalState.Seek => Random.Range(0, 100) < 40,
                _ => Random.Range(0, 100) < 25
            };
        }
        
        public void MarkEndChase(IAnimal animal)
        {
            if (_targetFood == null || animal != _targetFood ) return;

            SetState(new IdleState(this));
            _currentChaseCooldown = _ChaseCooldown;
            Debug.Log("Prey escaped");
        }
    }
}
