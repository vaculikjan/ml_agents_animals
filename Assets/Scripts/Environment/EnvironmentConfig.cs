// Author: Jan Vaculik

using System;
using Newtonsoft.Json;
using TrainingUtils;
using UnityEngine;

namespace Environment
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "Environment/EnvironmentConfig")]
    [JsonObject(MemberSerialization.OptIn)]
    public class EnvironmentConfig : ScriptableObject
    {
        [JsonProperty("output_path")]
        [SerializeField]
        private string _OutputPath;
        
        [JsonProperty("deer_config")]
        [SerializeField]
        private DeerConfig _DeerConfig;
        
        [JsonProperty("wolf_config")]
        [SerializeField]
        private WolfConfig _WolfConfig;
        
        [JsonProperty("flee_state_reward")]
        [SerializeField]
        private float _FleeStateReward;
        
        [JsonProperty("attack_state_reward")]
        [SerializeField]
        private float _AttackStateReward;
        
        [JsonProperty("pursuit_range")]
        [SerializeField]
        private float _PursuitRange;
        
        public string OutputPath => _OutputPath;
        public DeerConfig DeerConfig => _DeerConfig;
        public WolfConfig WolfConfig => _WolfConfig;
        public float FleeStateReward => _FleeStateReward;
        public float AttackStateReward => _AttackStateReward;
        public float PursuitRange => _PursuitRange;
        
        public ILogData LogData()
        {
            return new EnvironmentLogData
            {
                DeerLogData = _DeerConfig.LogData() as DeerConfig.DeerLogData,
                WolfLogData = _WolfConfig.LogData() as WolfConfig.WolfLogData
            };
        }
    }

    public class EnvironmentLogData : ILogData
    {
        public DeerConfig.DeerLogData DeerLogData { get; set; }
        public WolfConfig.WolfLogData WolfLogData { get; set; }
    }
    
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class DeerConfig : IAgentConfig
    {
        [JsonProperty("deer_count")]
        [SerializeField]
        private int _Count;
        
        [JsonProperty("deer_min_spawn_time")]
        [SerializeField]
        private float _MinSpawnTime;
        
        [JsonProperty("deer_max_spawn_time")]
        [SerializeField]
        private float _MaxSpawnTime;
        
        [JsonProperty("deer_move_speed")]
        [SerializeField]
        private float _MoveSpeed;
        
        [JsonProperty("deer_acceleration_rate")]
        [SerializeField]
        private float _AccelerationRate;
        
        [JsonProperty("deer_hunger_per_second")]
        [SerializeField]
        private float _HungerPerSecond;
        
        [JsonProperty("deer_hunger_reward_curve")]
        [SerializeField]
        private float[] _HungerRewardCurve;
        
        [JsonProperty("deer_energy_per_second")]
        [SerializeField]
        private float _EnergyPerSecond;
        
        [JsonProperty("deer_energy_regen_per_second")]
        [SerializeField]
        private float _EnergyRegenPerSecond;
        
        [JsonProperty("deer_energy_reward_curve")]
        [SerializeField]
        private float[] _EnergyRewardCurve;
        
        [JsonProperty("deer_curiosity_per_second")]
        [SerializeField]
        private float _CuriosityPerSecond;
        
        [JsonProperty("deer_curiosity_decay_per_second")]
        [SerializeField]
        private float _CuriosityDecayPerSecond;
        
        [JsonProperty("deer_curiosity_reward_curve")]
        [SerializeField]
        private float[] _CuriosityRewardCurve;
        
        [JsonProperty("deer_time_to_sleep")]
        [SerializeField]
        private float _TimeToSleep;
        
        [JsonProperty("deer_food_detection_radius")]
        [SerializeField]
        private float _FoodDetectionRadius;
        
        [JsonProperty("deer_food_consume_radius")]
        [SerializeField]
        private float _FoodConsumeRadius;
        
        [JsonProperty("deer_max_life_span")]
        [SerializeField]
        private float _MaxLifeSpan;
        
        [JsonProperty("deer_min_life_span")]
        [SerializeField]
        private float _MinLifeSpan;
        
        [JsonProperty("deer_threat_detection_radius")]
        [SerializeField]
        private float _ThreatDetectionRadius;
        
        [JsonProperty("deer_threat_radius_offset")]
        [SerializeField]
        private float _ThreatRadiusOffset;
        
        [JsonProperty("deer_threat_detection_interval")]
        [SerializeField]
        private int _ThreatDetectionInterval;
        
        [JsonProperty("deer_flee_acceleration_multiplier")]
        [SerializeField]
        private float _FleeAccelMultiplier;
        
        [JsonProperty("deer_flee_energy_multiplier")]
        [SerializeField]
        private float _FleeEnergyMultiplier;
        
        [JsonProperty("deer_detect_food_reward")]
        [SerializeField]
        private float _DetectFoodReward;
        
        [JsonProperty("deer_eat_food_reward")]
        [SerializeField]
        private float _EatFoodReward;
        
        [JsonProperty("deer_sleep_reward")]
        [SerializeField]
        private float _SleepReward;
        
        [JsonProperty("deer_starvation_reward")]
        [SerializeField]
        private float _StarvationReward;
        
        [JsonProperty("deer_natural_death_reward")]
        [SerializeField]
        private float _NaturalDeathReward;
        
        [JsonProperty("deer_eaten_reward")]
        [SerializeField]
        private float _EatenReward;
        
        [JsonProperty("deer_exhaustion_threshold")]
        [SerializeField]
        private float _ExhaustionThreshold;
        
        [JsonProperty("deer_exhaustion_slowdown")]  
        [SerializeField]
        private float _ExhaustionSlowdown;
        
        [JsonProperty("deer_state_memory_size")]
        [SerializeField]
        private int _StateMemorySize;
        
        [JsonProperty("deer_time_to_eat")]
        [SerializeField]
        private float _TimeToEat;
        
        public int Count => _Count;
        public float MinSpawnTime => _MinSpawnTime;
        public float MaxSpawnTime => _MaxSpawnTime;
        
        public float MoveSpeed => _MoveSpeed;
        public float AccelerationRate => _AccelerationRate;
        
        public float HungerPerSecond => _HungerPerSecond;
        public float[] HungerRewardCurve => _HungerRewardCurve;
        
        public float EnergyPerSecond => _EnergyPerSecond;
        public float EnergyRegenPerSecond => _EnergyRegenPerSecond;
        public float[] EnergyRewardCurve => _EnergyRewardCurve;
        
        public float CuriosityPerSecond => _CuriosityPerSecond;
        public float CuriosityDecayPerSecond => _CuriosityDecayPerSecond;
        public float[] CuriosityRewardCurve => _CuriosityRewardCurve;
        
        public float TimeToSleep => _TimeToSleep;
        public float FoodDetectionRadius => _FoodDetectionRadius;
        public float FoodConsumeRadius => _FoodConsumeRadius;
        public float MaxLifeSpan => _MaxLifeSpan;
        public float MinLifeSpan => _MinLifeSpan;
        
        public float ThreatDetectionRadius => _ThreatDetectionRadius;
        public float ThreatRadiusOffset => _ThreatRadiusOffset;
        public int ThreatDetectionInterval => _ThreatDetectionInterval;
        public float FleeAccelMultiplier => _FleeAccelMultiplier;
        public float FleeEnergyMultiplier => _FleeEnergyMultiplier;
        
        public float DetectFoodReward => _DetectFoodReward;
        public float EatFoodReward => _EatFoodReward;
        public float SleepReward => _SleepReward;
        
        public float StarvationReward => _StarvationReward;
        public float NaturalDeathReward => _NaturalDeathReward;
        public float EatenReward => _EatenReward;
        
        public float ExhaustionThreshold => _ExhaustionThreshold;
        public float ExhaustionSlowdown => _ExhaustionSlowdown;
        
        public int StateMemorySize => _StateMemorySize;
        
        public float TimeToEat => _TimeToEat;
        
        public ILogData LogData()
        {
            return new DeerLogData
            {
                Count = _Count,
                HungerPerSecond = _HungerPerSecond,
                EnergyPerSecond = _EnergyPerSecond,
                EnergyRegenPerSecond = _EnergyRegenPerSecond
            };
        }
        
        public class DeerLogData : ILogData
        {
            public int Count { get; set; }
            public float HungerPerSecond { get; set; }
            public float EnergyPerSecond { get; set; }
            public float EnergyRegenPerSecond { get; set; }
        }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class WolfConfig : IAgentConfig
    {
        [JsonProperty("wolf_count")]
        [SerializeField]
        private int _Count;
        
        [JsonProperty("wolf_min_spawn_time")]
        [SerializeField]
        private float _MinSpawnTime;
        
        [JsonProperty("wolf_max_spawn_time")]
        [SerializeField]
        private float _MaxSpawnTime;
        
        [JsonProperty("wolf_move_speed")]
        [SerializeField]
        private float _MoveSpeed;
        
        [JsonProperty("wolf_acceleration_rate")]
        [SerializeField]
        private float _AccelerationRate;
        
        [JsonProperty("wolf_hunger_per_second")]
        [SerializeField]
        private float _HungerPerSecond;
        
        [JsonProperty("wolf_hunger_reward_curve")]
        [SerializeField]
        private float[] _HungerRewardCurve;
        
        [JsonProperty("wolf_energy_per_second")]
        [SerializeField]
        private float _EnergyPerSecond;
        
        [JsonProperty("wolf_energy_regen_per_second")]
        [SerializeField]
        private float _EnergyRegenPerSecond;
        
        [JsonProperty("wolf_energy_reward_curve")]
        [SerializeField]
        private float[] _EnergyRewardCurve;
        
        [JsonProperty("wolf_curiosity_per_second")]
        [SerializeField]
        private float _CuriosityPerSecond;
        
        [JsonProperty("wolf_curiosity_decay_per_second")]
        [SerializeField]
        private float _CuriosityDecayPerSecond;
        
        [JsonProperty("wolf_curiosity_reward_curve")]
        [SerializeField]
        private float[] _CuriosityRewardCurve;
        
        [JsonProperty("wolf_time_to_sleep")]
        [SerializeField]
        private float _TimeToSleep;
        
        [JsonProperty("wolf_food_detection_radius")]
        [SerializeField]
        private float _FoodDetectionRadius;
        
        [JsonProperty("wolf_food_consume_radius")]
        [SerializeField]
        private float _FoodConsumeRadius;
        
        [JsonProperty("wolf_max_life_span")]
        [SerializeField]
        private float _MaxLifeSpan;
        
        [JsonProperty("wolf_min_life_span")]
        [SerializeField]
        private float _MinLifeSpan;
        
        [JsonProperty("wolf_attack_range")]
        [SerializeField]
        private float _AttackRange;
        
        [JsonProperty("wolf_pursuit_accel_multiplier")]
        [SerializeField]
        private float _PursuitAccelMultiplier;
        
        [JsonProperty("wolf_pursuit_energy_multiplier")]
        [SerializeField]
        private float _PursuitEnergyMultiplier;
        
        [JsonProperty("wolf_detect_food_reward")]
        [SerializeField]
        private float _DetectFoodReward;
        
        [JsonProperty("wolf_eat_food_reward")]
        [SerializeField]
        private float _EatFoodReward;
        
        [JsonProperty("wolf_sleep_reward")]
        [SerializeField]
        private float _SleepReward;
        
        [JsonProperty("wolf_starvation_reward")]
        [SerializeField]
        private float _StarvationReward;
        
        [JsonProperty("wolf_natural_death_reward")]
        [SerializeField]
        private float _NaturalDeathReward;
        
        [JsonProperty("wolf_exhaustion_threshold")]
        [SerializeField]
        private float _ExhaustionThreshold;
        
        [JsonProperty("wolf_exhaustion_slowdown")]
        [SerializeField]
        private float _ExhaustionSlowdown;
        
        [JsonProperty("wolf_state_memory_size")]
        [SerializeField]
        private int _StateMemorySize;
        
        public int Count => _Count;
        public float MinSpawnTime => _MinSpawnTime;
        public float MaxSpawnTime => _MaxSpawnTime;
        
        public float MoveSpeed => _MoveSpeed;
        public float AccelerationRate => _AccelerationRate;
        
        public float HungerPerSecond => _HungerPerSecond;
        public float[] HungerRewardCurve => _HungerRewardCurve;
        
        public float EnergyPerSecond => _EnergyPerSecond;
        public float EnergyRegenPerSecond => _EnergyRegenPerSecond;
        public float[] EnergyRewardCurve => _EnergyRewardCurve;
        
        public float CuriosityPerSecond => _CuriosityPerSecond;
        public float CuriosityDecayPerSecond => _CuriosityDecayPerSecond;
        public float[] CuriosityRewardCurve => _CuriosityRewardCurve;
        
        public float TimeToSleep => _TimeToSleep;
        public float FoodDetectionRadius => _FoodDetectionRadius;
        public float FoodConsumeRadius => _FoodConsumeRadius;
        public float MaxLifeSpan => _MaxLifeSpan;
        public float MinLifeSpan => _MinLifeSpan;
        
        public float AttackRange => _AttackRange;
        public float PursuitAccelMultiplier => _PursuitAccelMultiplier;
        public float PursuitEnergyMultiplier => _PursuitEnergyMultiplier;
        
        public float DetectFoodReward => _DetectFoodReward;
        public float EatFoodReward => _EatFoodReward;
        public float SleepReward => _SleepReward;
        
        public float StarvationReward => _StarvationReward;
        public float NaturalDeathReward => _NaturalDeathReward;
        
        public float ExhaustionThreshold => _ExhaustionThreshold;
        public float ExhaustionSlowdown => _ExhaustionSlowdown;
        
        public int StateMemorySize => _StateMemorySize;
        
        public ILogData LogData()
        {
            return new WolfLogData
            {
                Count = _Count,
                HungerPerSecond = _HungerPerSecond,
                EnergyPerSecond = _EnergyPerSecond,
                EnergyRegenPerSecond = _EnergyRegenPerSecond
            };
        }
        
        public class WolfLogData : ILogData
        {
            public int Count { get; set; }
            public float HungerPerSecond { get; set; }
            public float EnergyPerSecond { get; set; }
            public float EnergyRegenPerSecond { get; set; }
        }
    }
    
    public interface IAgentConfig
    {
        public int Count { get; }
        public float MinSpawnTime { get; }
        public float MaxSpawnTime { get; }
        
        public float MoveSpeed { get; }
        public float AccelerationRate { get; }
        
        public float HungerPerSecond { get; }
        public float[] HungerRewardCurve { get; }
        
        public float EnergyPerSecond { get; }
        public float EnergyRegenPerSecond { get; }
        public float[] EnergyRewardCurve { get; }
        
        public float CuriosityPerSecond { get; }
        public float CuriosityDecayPerSecond { get; }
        public float[] CuriosityRewardCurve { get; }
        
        public float TimeToSleep { get; }
        public float FoodDetectionRadius { get; }
        public float FoodConsumeRadius { get; }
        
        public float MaxLifeSpan { get; }
        public float MinLifeSpan { get; }
        
        public float DetectFoodReward { get; }
        public float EatFoodReward { get; }
        public float SleepReward { get; }
        
        public float StarvationReward { get; }
        public float NaturalDeathReward { get; }
        
        public float ExhaustionThreshold { get; }
        public float ExhaustionSlowdown { get; }
        
        public int StateMemorySize { get; }
    }
}
