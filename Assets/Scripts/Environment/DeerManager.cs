// Author: Jan Vaculik

using Agents;
using TrainingUtils;

namespace Environment
{
    public class DeerManager : AAgentSpawner<Deer>
    {
        public override Deer Spawn()
        {
            var deer = base.Spawn();
            deer.Died += OnDeerDied;
            return deer;
        }

        private void OnDeerDied(AAnimal<IDeerEdible> animal, DeathType deathtype)
        {
            LogLifespanOnDeath(animal.TimeLiving, animal.CurrentLifeSpan);
            
            switch (deathtype)
            {
                case DeathType.Fatigue:
                    break;
                case DeathType.Starvation:
                    animal.SetReward(-1000f);
                    break;
                case DeathType.Natural:
                    break;
                case DeathType.Eaten:
                    animal.SetReward(-100f);
                    break;
                default:
                    break;
            }
            
            if (Agents.Contains(animal as Deer))
                Agents.Remove(animal as Deer);
            
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
    }
}
