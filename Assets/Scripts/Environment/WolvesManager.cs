// Author: Jan Vaculik

using System;
using System.IO;
using Agents;
using TrainingUtils;
using UnityEngine;

namespace Environment
{
    public class WolvesManager : AAgentSpawner<Wolf>
    {
        public override Wolf Spawn()
        {
            var wolf = base.Spawn();
            wolf.Died += OnWolfDied;
            return wolf;
        }

        private void OnWolfDied(AAnimal<IWolfEdible> animal, DeathType deathtype)
        {
            LogLifespanOnDeath(animal.TimeLiving, animal.CurrentLifeSpan);

            var logMessage = $"Wolf died: {deathtype} - {animal.TimeLiving} - Timestamp: {DateTime.UtcNow}";
    
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
            }
    
            animal.AddReward(animal.TimeLiving / animal.CurrentLifeSpan * _NaturalDeathReward);
            Debug.Log($"Wolf died: {deathtype} - {animal.TimeLiving}");
    
            if (Agents.Contains(animal as Wolf))
                Agents.Remove(animal as Wolf);
    
            Destroy(animal.gameObject);
    
            if (Agents.Count == 0)
                SpawnAgent();
        }
        
        public ILogData LogData()
        {
            GetLifespanData(out var averageLifespan, out var averageLifespanDifference, out var lastLifespan);
            
            return new WolfLogData
            {
                AverageLifespan = averageLifespan,
                AverageLifespanDifference = averageLifespanDifference,
                LastLifespan = lastLifespan
            };
        }
        
        private class WolfLogData : ILogData
        {
            public float AverageLifespan { get; set; }
            public float AverageLifespanDifference { get; set; }
            public float LastLifespan { get; set; }
        }
    }
}
