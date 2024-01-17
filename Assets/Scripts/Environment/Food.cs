// Author: Jan Vaculik

using UnityEngine;

namespace Environment
{
    public class Food : MonoBehaviour, IDeerEdible
    {
        [SerializeField]
        private float _FoodValue = 0.5f;
    
        [SerializeField]
        private int _FoodCount;
    
        [SerializeField]
        private float _TimeToEat;
        public float TimeToEat => _TimeToEat;
        
        public event FoodEventHandler FoodDepleted = delegate {  };
    
        public float Eat() 
        { 
            _FoodCount--;
            
            if (_FoodCount <= 0) 
                FoodDepleted(this);

            return _FoodValue;
        }
        
        public MonoBehaviour GetSelf()
        {
            return this;
        }
    }
}