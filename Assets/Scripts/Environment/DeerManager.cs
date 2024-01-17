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
                    break;
                case DeathType.Natural:
                    break;
                case DeathType.Eaten:
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
