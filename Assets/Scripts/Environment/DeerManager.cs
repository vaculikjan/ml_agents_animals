// Author: Jan Vaculik

using Agents;

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
            SpawnAgent();
        }
    }
}
