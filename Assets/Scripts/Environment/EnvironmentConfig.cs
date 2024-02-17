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
        
        [JsonProperty("deer_count")]
        [SerializeField]
        private int _DeerCount;
        
        [JsonProperty("deer_hunger_per_second")]
        [SerializeField]
        private float _DeerHungerPerSecond;
        
        [JsonProperty("wolf_count")]
        [SerializeField]
        private int _WolfCount;
        
        [JsonProperty("wolf_hunger_per_second")]
        [SerializeField]
        private float _WolfHungerPerSecond;
        
        public string OutputPath => _OutputPath;
        public int DeerCount => _DeerCount;
        public float DeerHungerPerSecond => _DeerHungerPerSecond;
        public int WolfCount => _WolfCount;
        public float WolfHungerPerSecond => _WolfHungerPerSecond;
        
        public ILogData LogData()
        {
            return new EnvironmentLogData
            {
                DeerCount = _DeerCount,
                DeerHungerPerSecond = _DeerHungerPerSecond,
                WolfCount = _WolfCount,
                WolfHungerPerSecond = _WolfHungerPerSecond
            };
        }
    }

    public class EnvironmentLogData : ILogData
    {
        public int DeerCount { get; set; }
        public float DeerHungerPerSecond { get; set; }
        public int WolfCount { get; set; }
        public float WolfHungerPerSecond { get; set; }
    }
}
