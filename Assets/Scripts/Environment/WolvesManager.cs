// Author: Jan Vaculik

using Agents;
using TrainingUtils;

namespace Environment
{
    public class WolvesManager : AAgentSpawner<Wolf>
    {
        public override Wolf Spawn()
        {
            var deer = base.Spawn();
            deer.Died += OnWolfDied;
            return deer;
        }

        private void OnWolfDied(AAnimal<IWolfEdible> animal, DeathType deathtype)
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
                default:
                    break;
            }
            
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
