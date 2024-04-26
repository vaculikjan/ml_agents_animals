// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using Environment;
using StateMachine.AnimalStates;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace AI
{
    public class AIDeer : AAIAnimal<IDeerEdible>, IWolfEdible
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
        [SerializeField]
        private float _FoodValue = 1f;
        [SerializeField]
        private float _TimeToEat = 1f;
        
        private bool Fleeing => CurrentState?.StateID == AnimalState.Flee;
        private IThreat<AIWolf>[] _threats;
        private bool _dying;
        
        private void HandleThreatDetection()
        {
            if (FixedUpdateCounter % _ThreatDetectionInterval != 0) return;
            DetectThreats();
        }
        
        private void DetectThreats()
        {
            var foundColliders = Physics.OverlapSphere(transform.position + new Vector3(0,0, _ThreatRadiusOffset), _ThreatDetectionRadius);
            
            var foundThreats = foundColliders.Select(threat => threat.GetComponent<IThreat<AIWolf>>()).Where(threatComponent => threatComponent != null).ToList();

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
                var wolfComponent = foundThreat.GetSelf().GetComponent<AIWolf>();
                wolfComponent.Died += OnWolfDied;
            }
        }
        
        private void OnWolfDied(AAIAnimal<IWolfEdible> animal, DeathType deathtype)
        {
            _threats = _threats.Where(x => x != null && !x.Equals(null)).ToArray();
        }
        
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
        
        protected override void LoadFromConfig(IAgentConfig config)
        {
            base.LoadFromConfig(config);
            if (config is not DeerConfig deerConfig) return;

            _ThreatDetectionRadius = deerConfig.ThreatDetectionRadius;
            _FleeAccelerationMultiplier = deerConfig.FleeAccelMultiplier;
            _FleeEnergyMultiplier = deerConfig.FleeEnergyMultiplier;
            _ThreatDetectionInterval = deerConfig.ThreatDetectionInterval;
            _ThreatRadiusOffset = deerConfig.ThreatRadiusOffset;
            _SafetyDistance = AIEnvironmentController.Instance.EnvironmentConfig.PursuitRange + 5f;
            _TimeToEat = deerConfig.TimeToEat;
        }

        protected override void OnFoodDepleted(IEdible food)
        {
            FoodList.Remove(food as IDeerEdible);
            
            AvailableFood = AvailableFood.Where(x => x != null && !x.Equals(null)).ToArray();
            
            if (AvailableFood.Contains(food))
            {
                AvailableFood[Array.IndexOf(AvailableFood, food as IDeerEdible)] = null;
            }
            
            if (NearestFood == food)
            {
                NearestFood = null;
            }
        }

        protected override void HandleHunger()
        {
            if (CurrentState.StateID == AnimalState.Eat)
            {
                return;
            }
            
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
        
        protected override void FixedUpdate()
        {
            if (_dying)
            {
                return;
            }
            base.FixedUpdate();
            HandleThreatDetection();
            if (FixedUpdateCounter % 20 == 0)
            {
                if (CurrentState.StateID == AnimalState.Flee)
                {
                    _Energy.Value -= _EnergyPerSecond * _FleeEnergyMultiplier;
                }
            }
        }

        public override void Initialize()
        {
            LoadFromConfig(AIEnvironmentController.Instance.EnvironmentConfig.DeerConfig);
            base.Initialize();
            
            AvailableFood = new IDeerEdible[StateParams[AnimalState.Seek].ActionMap.Count];
            _threats = new IThreat<AIWolf>[StateParams[AnimalState.Flee].ActionMap.Count];
        }

        public float Eat()
        {
            FoodDepleted(this);
            OnDeath(DeathType.Eaten);
            return _FoodValue;
        }

        public float TimeToEat => _TimeToEat;
        public MonoBehaviour GetSelf() { return this; }

        public event FoodEventHandler FoodDepleted = delegate { };

        public void GetAttacked()
        {
            SetState(new IdleState(this)); 
            _dying = true;
        }

        public IAnimal GetAnimal() { return this; }
    }
}
