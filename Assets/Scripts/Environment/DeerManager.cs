// Author: Jan Vaculik

using System;
using System.IO;
using Agents;
using TrainingUtils;
using UnityEngine;

namespace Environment
{
    public class DeerManager : AAgentSpawner<Deer>
    {
        [SerializeField]
        private float _EatenReward;
        
        public override Deer Spawn()
        {
            var deer = base.Spawn();
            deer.Died += OnDeerDied;
            return deer;
        }

        private void OnDeerDied(AAnimal<IDeerEdible> animal, DeathType deathtype)
        {
            LogLifespanOnDeath(animal.TimeLiving, animal.CurrentLifeSpan);

            var logMessage = $"Deer died: {deathtype} - {animal.TimeLiving} - Timestamp: {DateTime.UtcNow}";
    
            var directoryPath = string.IsNullOrEmpty(EnvironmentController.Instance.EnvironmentConfigPath) 
                ? Application.persistentDataPath 
                : Path.GetDirectoryName(EnvironmentController.Instance.EnvironmentConfigPath);
            if (directoryPath != null)
            {
                var logFilePath = Path.Combine(directoryPath, "deathLogs.txt");
    
                LogToFileAsync(logMessage, logFilePath);
            }

            switch (deathtype)
            {
                case DeathType.Fatigue:
                    break;
                case DeathType.Starvation:
                    animal.AddReward(_StarvationReward);
                    break;
                case DeathType.Natural:
                    break;
                case DeathType.Eaten:
                    animal.AddReward(_EatenReward);
                    break;
            }
    
            animal.AddReward(animal.TimeLiving / animal.CurrentLifeSpan * _NaturalDeathReward);
            Debug.Log($"Deer died: {deathtype} - {animal.TimeLiving}");
    
            if (Agents.Contains(animal as Deer))
                Agents.Remove(animal as Deer);
    
            if (animal && animal.gameObject)
                Destroy(animal.gameObject);
    
            if (Agents.Count == 0)
                SpawnAgent();
        }


        public ILogData LogData()
        {
            GetLifespanData(out var averageLifespan, out var averageLifespanDifference, out var lastLifespan);
            
            return new DeerLogData
            {
                AverageLifespan = averageLifespan,
                AverageLifespanDifference = averageLifespanDifference,
                LastLifespan = lastLifespan
            };
        }
        
        private class DeerLogData : ILogData
        {
            public float AverageLifespan { get; set; }
            public float AverageLifespanDifference { get; set; }
            public float LastLifespan { get; set; }
        }

        public override void Initialize(IAgentConfig agentConfig)
        {
            base.Initialize(agentConfig);
            if (agentConfig is DeerConfig deerConfig)
                _EatenReward = deerConfig.EatenReward;
        }
    }
}
