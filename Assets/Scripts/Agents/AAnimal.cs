// Author: Jan Vaculik

using System.Collections.Generic;
using System.Linq;
using AgentProperties.Attributes;
using Environment;
using StateMachine.AnimalStates;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
    public abstract class AAnimal<TEdible> : Agent, IAnimal where TEdible : IEdible
    {
        [Header("ML Agents Properties")]
        [SerializeField]
        protected BehaviorParameters _BehaviorParameters;
        
        [Header("Animal variables")]
        [SerializeField]
        private Rigidbody _AnimalRigidbody;
        [SerializeField]
        protected float _MovementSpeed;
        [SerializeField]
        protected float _RotationSpeed;
        
        [Header("Animal Attributes")]
        [SerializeField]
        protected AnimalAttribute _Hunger;
        [SerializeField]
        protected AnimalAttribute _Curiosity;
        [SerializeField]
        protected AnimalAttribute _Energy;
        
        [Header("Attribute Variables")]
        [SerializeField]
        protected float _HungerPerSecond;
        [SerializeField]
        protected float _EnergyPerSecond;
        [SerializeField]
        protected float _EnergyRegenPerSecond;
        [SerializeField]
        protected float _TimeToSleep;
        [SerializeField]
        protected float _CuriosityPerSecond;
        [SerializeField]
        protected float _CuriosityDecayPerSecond;
        
        [Header("Food Detection")]
        [SerializeField]
        protected float _FoodDetectionRadius;
        [SerializeField]
        protected float _FoodConsumeRadius;
        [SerializeField]
        protected BufferSensorComponent _FoodSensor;
        
        protected int MaxDiscreteStates => _BehaviorParameters.BrainParameters.ActionSpec.BranchSizes[0];
        
        protected abstract Dictionary<AnimalState, AnimalStateInfo> StateParams { get; }
        protected IAnimalState CurrentState;
        protected List<TEdible> FoodList;
        protected TEdible[] AvailableFood;
        protected TEdible NearestFood;
        
        public AnimalAttribute Hunger => _Hunger;
        public AnimalAttribute Curiosity => _Curiosity;
        public AnimalAttribute Energy => _Energy;
        public Rigidbody AnimalRigidbody => _AnimalRigidbody;
        
        public delegate void DeathEventHandler(AAnimal<TEdible> animal, DeathType deathType);
        public event DeathEventHandler Died = delegate {  };
        
        protected void SetStateMask(ref IDiscreteActionMask actionMask, AnimalState state)
        {
            foreach (var actionIndex in StateParams[state].ActionMap)
            {
                actionMask.SetActionEnabled(0,  actionIndex, true);
            }
        }

        private bool IsTransitionValid(IAnimalState newState)
        {
            if (CurrentState == null) return true;

            return StateParams.TryGetValue(CurrentState.StateID, out var stateInfo) && stateInfo.ValidTransitions.Contains(newState.StateID) && CurrentState.CanExit();
        }

        public void ResolveSleeping(float timeSlept)
        {
            Energy.Value += timeSlept * _EnergyRegenPerSecond;
        }
        
        public void ResolveEating(IEdible food)
        {
            Hunger.Value -= food.Eat();
        }
        
        public void DetectFood()
        { 
            var foundColliders = Physics.OverlapSphere(transform.position, _FoodDetectionRadius);
            foreach (var food in foundColliders)
            {
                var foodComponent = food.GetComponent<TEdible>();
                
                if (foodComponent == null) continue;
                if (FoodList.Contains(foodComponent)) continue;
                
                AddReward(5f);
                FoodList.Add(foodComponent);
                
                foodComponent.FoodDepleted += OnFoodDepleted;
            }
        }
        
        protected void CalculateClosestFood()
        {
            var selfPosition = transform.position;
            FoodList.Sort((x, y) => Vector3.Distance(selfPosition, x.GetSelf().transform.position).CompareTo(Vector3.Distance(selfPosition, y.GetSelf().transform.position)));
            
            for (var i = 0; i < AvailableFood.Length; i++)
            {
                if (i >= FoodList.Count)
                {
                    AvailableFood[i] = default;
                    continue;
                }
                AvailableFood[i] = FoodList[i];
            }
        }
        
        protected bool CanEatAvailableFood(out TEdible food)
        {
            food = default;
            
            foreach (var x in AvailableFood)
            {
                if (x == null) continue;
                if (!(Vector3.Distance(transform.position, x.GetSelf().transform.position) <= _FoodConsumeRadius)) continue;
                food = x;
                return true;
            }

            return false;
        }
        
        public GameObject GetSelf()
        {
            return gameObject;
        }
        
        public bool SetState(IAnimalState state)
        {
            if (!IsTransitionValid(state))
            {
                Debug.LogWarning($"Transition from {CurrentState.StateID} to {state.StateID} is not valid!");
                return false;
            }
            
            CurrentState?.Exit();
            CurrentState = state;
            CurrentState.Enter();

            return true;
        }

        public override void Initialize()
        {
            SetState(new IdleState(this));
            
            FoodList = new List<TEdible>();
            
            _Hunger.Reset();
            _Energy.Reset();
            _Curiosity.Reset();
        }

        protected virtual void OnDrawGizmos()
        {
            var position = transform.position;
            DrawCircle(position, _FoodDetectionRadius, Color.cyan);
            DrawCircle(position, _FoodConsumeRadius, Color.green);
        }

        protected void DrawCircle(Vector3 position, float radius, Color color, int segments = 360)
        {
            Gizmos.color = color;

            for (var i = 0; i < segments; i++)
            {
                var angle = i * Mathf.Deg2Rad;
                var x = position.x + Mathf.Cos(angle) * radius;
                var z = position.z + Mathf.Sin(angle) * radius;

                var nextAngle = (i + 1) * Mathf.Deg2Rad;
                var nextX = position.x + Mathf.Cos(nextAngle) * radius;
                var nextZ = position.z + Mathf.Sin(nextAngle) * radius;

                Gizmos.DrawLine(new Vector3(x, position.y, z), new Vector3(nextX, position.y, nextZ));
            }
        }
        
        protected void OnDeath(DeathType deathType)
        {
            Died(this, deathType);
        }

        protected abstract void OnFoodDepleted(IEdible food);
        
        protected void MaskState(AnimalState state, ref IDiscreteActionMask actionMask, int branch, bool enable)
        {
            foreach (var actionIndex in StateParams[state].ActionMap)
            {
                actionMask.SetActionEnabled(branch, actionIndex, enable);
            }
        }
        
        protected void MaskSeekState(ref IDiscreteActionMask actionMask)
        {
            for (var i = StateParams[AnimalState.Seek].ActionMap.First(); i <= StateParams[AnimalState.Seek].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, i - StateParams[AnimalState.Seek].ActionMap.First() < AvailableFood.Length && AvailableFood[i - StateParams[AnimalState.Seek].ActionMap.First()] != null);
            }
        }
    }
}
