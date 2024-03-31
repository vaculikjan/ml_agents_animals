// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.IO;
using TrainingUtils;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment
{
    public abstract class AAgentSpawner<T> : ASpawner<T> where T : Agent
    {
        [SerializeField]
        private int _AgentsCount;
        [SerializeField]
        private float _MinSpawnInterval;
        [SerializeField]
        private float _MaxSpawnInterval;
        [SerializeField]
        protected float _StarvationReward;
        [SerializeField]
        protected float _NaturalDeathReward;

        protected readonly List<T> Agents = new();

        private int _deadAgentCount;
        private float _totalLifespan;
        private float _totalLifespanDifference;
        private float _lastLifespan;
        private float _lastMaxLifespan;
        
        private float SpawnInterval => UnityEngine.Random.Range(_MinSpawnInterval, _MaxSpawnInterval);
        private float _timeToSpawn;

        public void ResetAgents()
        {
            foreach (var agent in Agents)
            {
                Destroy(agent.gameObject);
            }

            Agents.Clear();

            for (var i = 0; i < _AgentsCount; i++)
            {
                SpawnAgent();
            }
        }

        protected void SpawnAgent() { Agents.Add(Spawn()); }

        protected void LogLifespanOnDeath(float onDeathLifespan, float currentMaxLifespan)
        {
            _deadAgentCount++;
            _totalLifespan += onDeathLifespan;
            _totalLifespanDifference += currentMaxLifespan - _lastLifespan;
            _lastLifespan = onDeathLifespan;
            _lastMaxLifespan = currentMaxLifespan;
        }

        protected void GetLifespanData(out float averageLifespan, out float averageLifespanDifference, out float lastLifespan)
        {
            averageLifespan = _totalLifespan / _deadAgentCount;
            averageLifespanDifference = _totalLifespanDifference / _deadAgentCount;
            lastLifespan = _lastLifespan;
        }

        private void Update()
        {
            if (Agents.Count >= _AgentsCount) return;
            
            if (_timeToSpawn < SpawnInterval)
            {
                _timeToSpawn += Time.fixedDeltaTime;
            }
            else
            {
                SpawnAgent();
                _timeToSpawn = 0;
            }
        }
        
        public virtual void Initialize(IAgentConfig agentConfig)
        {
            _AgentsCount = agentConfig.Count;
            _MinSpawnInterval = agentConfig.MinSpawnTime;
            _MaxSpawnInterval = agentConfig.MaxSpawnTime;
            _StarvationReward = agentConfig.StarvationReward;
            _NaturalDeathReward = agentConfig.NaturalDeathReward;
        }
        
        protected async void LogToFileAsync(string logMessage, string filePath)
        {
            try
            {
                await using StreamWriter streamWriter = File.AppendText(filePath);
                await streamWriter.WriteLineAsync(logMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to log data: {ex.Message}");
            }
        }

    }
}
