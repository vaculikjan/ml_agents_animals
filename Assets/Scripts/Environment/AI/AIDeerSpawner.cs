// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.IO;
using Agents;
using AI;
using UnityEngine;

namespace Environment
{
    public class AIDeerSpawner : AISpawner<AIDeer>
    {
        [SerializeField]
        private int _AgentsCount;
        [SerializeField]
        private float _MinSpawnInterval;
        [SerializeField]
        private float _MaxSpawnInterval;
        
        private List<AIDeer> _agents = new();
        
        public void ResetAgents()
        {
            foreach (var agent in _agents)
            {
                Destroy(agent.gameObject);
            }
            
            _agents.Clear();
            
            for (var i = 0; i < _AgentsCount; i++)
            {
                SpawnAgent();
            }
        }

        private void SpawnAgent()
        {
            var agent = Spawn();
            agent.Initialize();
            _agents.Add(agent);
        }
        
        public override AIDeer Spawn()
        {
            var agent = base.Spawn();
            agent.Died += OnAgentDied;
            return agent;
        }

        private void OnAgentDied(AAIAnimal<IDeerEdible> animal, DeathType deathtype) { 
            var logMessage = $"Deer died: {deathtype} - {animal.TimeLiving} - Timestamp: {DateTime.UtcNow}";
            var directoryPath = string.IsNullOrEmpty(AIEnvironmentController.Instance.EnvironmentConfigPath) 
                ? Application.persistentDataPath 
                : Path.GetDirectoryName(AIEnvironmentController.Instance.EnvironmentConfigPath);
            if (directoryPath != null)
            {
                var logFilePath = Path.Combine(directoryPath, "aiDeathLogs.txt");
                LogToFileAsync(logMessage, logFilePath);
            }
        }
    }
}
