// Author: Jan Vaculik

using UnityEngine;

namespace Environment
{
    public interface IEdible
    {
        public float Eat();
        public float TimeToEat { get; }
        public MonoBehaviour GetSelf();
        
        public event FoodEventHandler FoodDepleted;
    }
}
