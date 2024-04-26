// Author: Jan Vaculik

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AgentProperties.Attributes;
using Environment;
using StateMachine.AnimalStates;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;

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
        protected float _AccelerationRate;
        [SerializeField]
        protected float _RotationSpeed;
        [SerializeField]
        protected float _ExhaustionThreshold;
        [SerializeField]
        protected float _ExhaustionSlowdown;
        
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
        
        [Header("Lifespan")]
        [SerializeField]
        protected float _MaxLifeSpan = 180f;
        [SerializeField]
        protected float _MinLifeSpan = 120f;
        
        [Header("Other members")]
        [SerializeField]
        private BufferSensorComponent _SpeciesSensor;
        
        [Header("State Memory")]
        [SerializeField]
        private int _PastStatesCount;
        
        private float _lastFixedTime;
        private float _currentAcceleration;
        private float _maxAcceleration = 1f;
        
        protected int MaxDiscreteStates => _BehaviorParameters.BrainParameters.ActionSpec.BranchSizes[0];
        public float MovementSpeed => _Energy.Value > _ExhaustionThreshold ? _MovementSpeed : _MovementSpeed * _ExhaustionSlowdown;
        public float RotationSpeed => _RotationSpeed;
        
        protected abstract Dictionary<AnimalState, AnimalStateInfo> StateParams { get; }
        public IAnimalState CurrentState { get; private set; }
        protected List<TEdible> FoodList;
        protected TEdible[] AvailableFood;
        protected TEdible NearestFood;
        protected int FixedUpdateCounter;

        protected Queue<float> StateMemory;
        
        // rewards
        protected float DetectFoodReward = 0.0f;
        private float _eatFoodReward = 0.0f;
        private float _sleepReward = 0.0f;

        public float CurrentLifeSpan { get; private set; }
        public float TimeLiving { get; private set; }

        public AnimalAttribute Hunger => _Hunger;
        public AnimalAttribute Curiosity => _Curiosity;
        public AnimalAttribute Energy => _Energy;
        public Rigidbody AnimalRigidbody => _AnimalRigidbody;
        
        public delegate void DeathEventHandler(AAnimal<TEdible> animal, DeathType deathType);
        public event DeathEventHandler Died = delegate {  };

        protected bool Transitioning;
        
        protected virtual void LoadFromConfig(IAgentConfig config)
        {
            _MovementSpeed = config.MoveSpeed;
            _AccelerationRate = config.AccelerationRate;
            
            _Hunger.SetCurveFromArray(config.HungerRewardCurve); 
            _Energy.SetCurveFromArray(config.EnergyRewardCurve);
            _Curiosity.SetCurveFromArray(config.CuriosityRewardCurve);
            
            _TimeToSleep = config.TimeToSleep;
            _HungerPerSecond = config.HungerPerSecond;
            _EnergyPerSecond = config.EnergyPerSecond;
            _EnergyRegenPerSecond = config.EnergyRegenPerSecond;
            _CuriosityPerSecond = config.CuriosityPerSecond;
            _CuriosityDecayPerSecond = config.CuriosityDecayPerSecond;
            
            _FoodDetectionRadius = config.FoodDetectionRadius;
            _FoodConsumeRadius = config.FoodConsumeRadius;
            
            _MaxLifeSpan = config.MaxLifeSpan;
            _MinLifeSpan = config.MinLifeSpan;
            
            DetectFoodReward = config.DetectFoodReward;
            _eatFoodReward = config.EatFoodReward;
            _sleepReward = config.SleepReward;
            
            _ExhaustionThreshold = config.ExhaustionThreshold;
            _ExhaustionSlowdown = config.ExhaustionSlowdown;

            _PastStatesCount = config.StateMemorySize;
        }
        
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
        
        protected void HandleLifeSpan()
        {
            TimeLiving += Time.fixedDeltaTime;
            if (TimeLiving < CurrentLifeSpan) return;
            
            OnDeath(DeathType.Natural);
        }
        
        public void ResolveSleeping(float timeSlept)
        {
            if (Energy.Value <= 0.5f)
                AddReward(_sleepReward);
            Energy.Value += timeSlept * _EnergyRegenPerSecond;
        }
        
        public void ResolveEating(IEdible food)
        {
            if (Hunger.Value >= 0.5f)
                AddReward(_eatFoodReward);
            Hunger.Value -= food.Eat();
        }
        
        private void DetectOtherAnimalsOfSpecies()
        {
            var foundColliders = Physics.OverlapSphere(transform.position, _FoodDetectionRadius);
            
            foreach (var animal in foundColliders)
            {
                if (!animal || animal.Equals(null)) continue;
                
                var animalComponent = animal.GetComponent<IAnimal>();
                if (animalComponent == null || animalComponent.Equals(null) || animalComponent.Equals(this)) continue;
                
                var distanceToAnimal = Vector3.Distance(transform.position, animal.transform.position);
                
                var animalObservation = new float[]
                {
                    distanceToAnimal,
                    (float) animalComponent.CurrentState.StateID,
                };
                
                _SpeciesSensor.AppendObservation(animalObservation);
            }
        }
        
        public virtual void DetectFood()
        { 
            var foundColliders = Physics.OverlapSphere(transform.position, _FoodDetectionRadius);
            foreach (var food in foundColliders)
            {
                var foodComponent = food.GetComponent<TEdible>();
                
                if (foodComponent == null) continue;
                if (FoodList.Contains(foodComponent)) continue;
                
                AddReward(DetectFoodReward);
                FoodList.Add(foodComponent);
                
                foodComponent.FoodDepleted += OnFoodDepleted;
            }
        }
        
        protected void CalculateClosestFood()
        {
            var selfPosition = transform.position;
            FoodList.RemoveAll(x => x == null || x.Equals(null) || !x.GetSelf() || x.GetSelf().Equals(null));
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

            if (transform == null) return false;
            
            foreach (var x in AvailableFood)
            {
                var xTransform = x?.GetSelf()?.transform;
                if (xTransform == null) continue;
                if (!(Vector3.Distance(transform.position, xTransform.position) <= _FoodConsumeRadius)) continue;
                food = x;
                return true;
            }

            return false;
        }
        
        public GameObject GetSelf()
        {
            return gameObject;
        }

        public float Acceleration
        {
            set => _currentAcceleration = value;
            
            get
            {
                if (_currentAcceleration <= 0f)
                {
                    _lastFixedTime = Time.fixedTime;
                    _currentAcceleration = _maxAcceleration / 100f;
                }

                var timeSinceLastFixedUpdate = Time.fixedTime - _lastFixedTime;
                _currentAcceleration += _AccelerationRate * timeSinceLastFixedUpdate;
                _currentAcceleration = Mathf.Min(_currentAcceleration, _maxAcceleration);
                _lastFixedTime = Time.fixedTime;
                return _currentAcceleration;
            }
        }

        public float MaxAcceleration { set => _maxAcceleration = value; }

        public bool SetState(IAnimalState state)
        {
            if (!IsTransitionValid(state))
            {
                Debug.LogWarning($"Transition from {CurrentState.StateID} to {state.StateID} is not valid!");
                return false;
            }
            
            if (this)
                StartCoroutine(SetStateRoutine(state));

            return true;
        }
        
        private IEnumerator SetStateRoutine(IAnimalState state)
        {
            if (!IsTransitionValid(state))
            {
                Debug.LogWarning($"Transition from {CurrentState.StateID} to {state.StateID} is not valid!");
                yield break;
            }
            
            Transitioning = true;
            if (CurrentState != null)
                yield return CurrentState?.ExitCoroutine();
            CurrentState = state;
            yield return CurrentState?.EnterCoroutine();
            Transitioning = false;
        }

        public override void Initialize()
        {
            _BehaviorParameters.BrainParameters.VectorObservationSize += _PastStatesCount;
            SetState(new IdleState(this));
            
            FoodList = new List<TEdible>();
            StateMemory = new Queue<float>(_PastStatesCount);
            for (var i = 0; i < _PastStatesCount; i++)
            {
                StateMemory.Enqueue(0);
            }
            
            if (CurrentState != null)
                AddStateToMemory((int) CurrentState.StateID);
            
            _Hunger.Reset();
            _Energy.Reset();
            _Curiosity.Reset();
            
            CurrentLifeSpan = Random.Range(_MinLifeSpan, _MaxLifeSpan);
            TimeLiving = 0f;
            _lastFixedTime = 0f;
        }

        protected virtual void OnDrawGizmos()
        {
            var position = transform.position;
            if (this is Deer deer)
                DrawCircle(position, _FoodDetectionRadius, Color.cyan);
            else
                DrawCircle(position, _FoodDetectionRadius, Color.red);
            
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
        
        protected void TryEnableSeekState(ref IDiscreteActionMask actionMask)
        {
            for (var i = StateParams[AnimalState.Seek].ActionMap.First(); i <= StateParams[AnimalState.Seek].ActionMap.Last(); i++)
            {
                actionMask.SetActionEnabled(0, i, i - StateParams[AnimalState.Seek].ActionMap.First() < AvailableFood.Length && AvailableFood[i - StateParams[AnimalState.Seek].ActionMap.First()] != null);
            }
        }
        
        protected void AddStateToMemory(int state)
        {
            if (StateMemory.Count >= _PastStatesCount)
            {
                StateMemory.Dequeue();
            }
            
            StateMemory.Enqueue(state);
        }

        protected abstract void HandleHunger();
        protected abstract void HandleEnergy();
        protected abstract void HandleCuriosity();
        
        protected virtual void FixedUpdate()
        {
            CalculateClosestFood();
            if (!Transitioning)
                CurrentState?.Execute();
            
            FixedUpdateCounter++;
            
            HandleLifeSpan();
            HandleHunger();
            HandleEnergy();
            HandleCuriosity();
            
            if (_SpeciesSensor != null)
                DetectOtherAnimalsOfSpecies();
        }
    }
}
