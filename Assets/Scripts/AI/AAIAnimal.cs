// Author: Jan Vaculik

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AgentProperties.Attributes;
using Agents;
using Environment;
using StateMachine.AnimalStates;
using StateMachine.Transitions;
using UnityEngine;

namespace AI
{
    public abstract class AAIAnimal<TEdible> : MonoBehaviour, IAIAnimal, IAnimal where TEdible : IEdible
    {
        [SerializeField]
        private Rigidbody _AnimalRigidbody;
        
        [SerializeField]
        private float _MovementSpeed;
        
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
        
        [Header("Lifespan")]
        [SerializeField]
        protected float _MaxLifeSpan = 180f;
        [SerializeField]
        protected float _MinLifeSpan = 120f;
        
        [Header("Transition Rules")]
        [SerializeField]
        protected List<TransitionSet> TransitionSets;
        
        protected abstract Dictionary<AnimalState, AnimalStateInfo> StateParams { get; }
        protected List<TEdible> FoodList;
        protected TEdible[] AvailableFood;
        protected TEdible NearestFood;
        protected bool Transitioning;
        protected int FixedUpdateCounter;
        
        protected bool Resting => CurrentState?.StateID == AnimalState.Sleep;
        
        public float MovementSpeed => _Energy.Value > _ExhaustionThreshold ? _MovementSpeed : _MovementSpeed * _ExhaustionSlowdown;
        public float RotationSpeed => _RotationSpeed;
        public IAnimalState CurrentState { get; private set; }
        public float MaxAcceleration { set => _maxAcceleration = value; }
        
        public bool FoodAvailable => AvailableFood.Length > 0;

        public Rigidbody AnimalRigidbody => _AnimalRigidbody;
        public AnimalAttribute Hunger => _Hunger;
        public AnimalAttribute Energy => _Energy;
        public float TimeLiving { get; private set; }
        public float CurrentLifeSpan { get; private set; }
        
        public delegate void DeathEventHandler(AAIAnimal<TEdible> animal, DeathType deathType);
        public event DeathEventHandler Died = delegate {  };
        
        private float _lastFixedTime;
        private float _currentAcceleration;
        private float _maxAcceleration = 1f;

        public GameObject GetSelf() => gameObject;

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

        public void ResolveSleeping(float timeSlept) { Energy.Value += timeSlept * _EnergyRegenPerSecond; }

        public void ResolveEating(IEdible food) { Hunger.Value -= food.Eat(); }
        private void HandleLifeSpan()
        {
            TimeLiving += Time.fixedDeltaTime;
            if (TimeLiving < CurrentLifeSpan) return;
            
            OnDeath(DeathType.Natural);
        }
        
        protected abstract void OnFoodDepleted(IEdible food);
        
        protected abstract void HandleHunger();
        protected abstract void HandleEnergy();
        
        protected void OnDeath(DeathType deathType)
        {
            Died(this, deathType);
        }
       
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
           
           _ExhaustionThreshold = config.ExhaustionThreshold;
           _ExhaustionSlowdown = config.ExhaustionSlowdown;
        }
       
        private bool IsTransitionValid(IAnimalState newState)
        {
            if (CurrentState == null) return true;

            return StateParams.TryGetValue(CurrentState.StateID, out var stateInfo) && stateInfo.ValidTransitions.Contains(newState.StateID) && CurrentState.CanExit();
        }

        public virtual void DetectFood() { var foundColliders = Physics.OverlapSphere(transform.position, _FoodDetectionRadius);
           foreach (var food in foundColliders)
           {
               var foodComponent = food.GetComponent<TEdible>();
                
               if (foodComponent == null) continue;
               if (FoodList.Contains(foodComponent)) continue;
                
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
        
        public bool CanEatAvailableFood(out TEdible food)
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
        
        public bool CanEat() => CanEatAvailableFood(out NearestFood);

        public float GetAttributeValue(string attribute)
        {
            return attribute switch
            {
                "Hunger" => _Hunger.Value,
                "Energy" => _Energy.Value,
                "Curiosity" => _Curiosity.Value,
                _ => 0f
            };
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
        
        public virtual void Initialize()
        {
            SetState(new IdleState(this));
            
            FoodList = new List<TEdible>();
            
            _Hunger.Reset();
            _Energy.Reset();
            _Curiosity.Reset();
            
            CurrentLifeSpan = Random.Range(_MinLifeSpan, _MaxLifeSpan);
            TimeLiving = 0f;
            _lastFixedTime = 0f;
        }
        
        protected virtual void FixedUpdate()
        {
            FixedUpdateCounter++;
            CalculateClosestFood();
            if (!Transitioning)
                CurrentState?.Execute();
            
            HandleLifeSpan();
            HandleHunger(); 
            HandleEnergy();

            CheckTransitionRules();
        }
        
        protected void CheckTransitionRules()
        {
            var currentSet = TransitionSets.Where(x => x.State == CurrentState.StateID).ToList().FirstOrDefault();
            if (currentSet == null) return;
            
            var possibleTransition = currentSet.GetTransition(this, this);
            if (possibleTransition != null)
            {
                possibleTransition.MakeTransition(this, this);
            }
        }
    }
}
