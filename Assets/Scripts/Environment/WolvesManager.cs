// Author: Jan Vaculik

using Agents;

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
            SpawnAgent();
        }
    }
}
