// Author: Jan Vaculik

using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

namespace Environment
{
    public abstract class AAgentSpawner<T> : ASpawner<T> where T : Agent
    {
        [SerializeField]
        private int _AgentsCount;
        
        protected readonly List<T> Agents = new();
        
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
    }
}
