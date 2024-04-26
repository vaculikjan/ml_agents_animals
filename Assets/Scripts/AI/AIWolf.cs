// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using Agents;
using Environment;
using StateMachine.AnimalStates;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI
{
    public class AIWolf : AAIAnimal<IWolfEdible>, IThreat<AIWolf>
    {
        [Header("Wolf variables")]
        [SerializeField]
        private float _AttackRange;
        [SerializeField]
        private float _PursueAccelMultiplier;
        [SerializeField]
        private float _PursueEnergyMultiplier = 1.5f;
        [SerializeField]
        private float _PursuitRange = 100f;
        
        private bool Pursuing => CurrentState?.StateID == AnimalState.Pursue;
        
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
                ValidTransitions = new List<AnimalState> {AnimalState.None, AnimalState.Eat, AnimalState.Idle}, ActionMap = new List<int> {}
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
                var deerComponent = foodComponent as AIDeer;
                
                if (foodComponent == null || deerComponent == null) continue;
                if (FoodList.Contains(foodComponent)) continue;
                
                FoodList.Add(foodComponent);
                
                foodComponent.FoodDepleted += OnFoodDepleted;
                deerComponent.Died += OnFoodDied;
            }
        }
        
        private void OnFoodDied(AAIAnimal<IDeerEdible> animal, DeathType deathtype)
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

        public override void Initialize()
        {
            LoadFromConfig(AIEnvironmentController.Instance.EnvironmentConfig.WolfConfig);
            base.Initialize();
            AvailableFood = new IWolfEdible[StateParams[AnimalState.Seek].ActionMap.Count];
        }
        
        protected override void LoadFromConfig(IAgentConfig config)
        {
            base.LoadFromConfig(config);

            if (config is not WolfConfig wolfConfig) return;
            
            _AttackRange = wolfConfig.AttackRange;
            _PursueAccelMultiplier = wolfConfig.PursuitAccelMultiplier;
            _PursueEnergyMultiplier = wolfConfig.PursuitEnergyMultiplier;
            _PursuitRange = AIEnvironmentController.Instance.EnvironmentConfig.PursuitRange;
        }
        
        protected override void HandleHunger()
        {
            if (CurrentState.StateID == AnimalState.Eat)
                return;
            
            if (FixedUpdateCounter % 50 == 0)
            {
                _Hunger.Value += _HungerPerSecond * (Resting ? 0.3f : 1f);
            }

            if (_Hunger.Value < _Hunger.MaxValue) return;
            
            OnDeath(DeathType.Starvation);
        }
        
        protected override void HandleEnergy()
        {
            if (FixedUpdateCounter % 50 == 0)
            {
                _Energy.Value += _EnergyPerSecond * (Pursuing ? _PursueEnergyMultiplier : 1f);
            }
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
        
        public bool DetectThreat(out AIWolf animal)
        {
            animal = this;
            return CurrentState.StateID switch
            {
                AnimalState.Pursue => Random.Range(0, 100) < 75,
                AnimalState.Seek => Random.Range(0, 100) < 40,
                _ => Random.Range(0, 100) < 25
            };
        }

    }
}
