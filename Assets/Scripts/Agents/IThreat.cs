// Author: Jan Vaculik

using UnityEngine;

namespace Agents
{
    public interface IThreat<T> where T : IAnimal
    {
        public bool DetectThreat(out T animal);
        public GameObject GetSelf();
    }
}
