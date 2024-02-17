// Author: Jan Vaculik

using System.Collections.Generic;
using TrainingUtils;
using Unity.MLAgents;
using UnityEngine;

namespace Environment
{
    public abstract class AAgentSpawner<T> : ASpawner<T> where T : Agent
    {
        [SerializeField]
        private int _AgentsCount;

        protected readonly List<T> Agents = new();

        private int _deadAgentCount;
        private float _totalLifespan;
        private float _totalLifespanDifference;
        private float _lastLifespan;
        private float _lastMaxLifespan;

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
        
        public void GetLifespanData(out float averageLifespan, out float averageLifespanDifference, out float lastLifespan)
        {
            averageLifespan = _totalLifespan / _deadAgentCount;
            averageLifespanDifference = _totalLifespanDifference / _deadAgentCount;
            lastLifespan = _lastLifespan;
        }
    }
}
