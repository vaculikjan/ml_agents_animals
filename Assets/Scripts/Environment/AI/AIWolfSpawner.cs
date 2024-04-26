// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.IO;
using Agents;
using AI;
using UnityEngine;

namespace Environment
{
    public class AIWolfSpawner : AISpawner<AIWolf>
    {
        [SerializeField]
        private int _AgentsCount;
        [SerializeField]
        private float _MinSpawnInterval;
        [SerializeField]
        private float _MaxSpawnInterval;
        
        private List<AIWolf> _agents = new();
        
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

        public override AIWolf Spawn()
        {
            var agent = base.Spawn();
            agent.Died += OnAgentDied;
            return agent;
        }

        private void OnAgentDied(AAIAnimal<IWolfEdible> animal, DeathType deathtype)
        {
            var logMessage = $"Wolf died: {deathtype} - {animal.TimeLiving} - Timestamp: {DateTime.UtcNow}";
    
            var directoryPath = string.IsNullOrEmpty(AIEnvironmentController.Instance.EnvironmentConfigPath) 
                ? Application.persistentDataPath 
                : Path.GetDirectoryName(AIEnvironmentController.Instance.EnvironmentConfigPath);
            if (directoryPath != null)
            {
                var logFilePath = Path.Combine(directoryPath, "aiDeathLog.txt");
    
                LogToFileAsync(logMessage, logFilePath);
            }
        }
    }
}
