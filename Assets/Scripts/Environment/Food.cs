// Author: Jan Vaculik

using UnityEngine;

namespace Environment
{
    public class Food : MonoBehaviour
    {
        [SerializeField]
        private float _FoodValue = 10;
    
        [SerializeField]
        private int _FoodCount;
    
        [SerializeField]
        private float _TimeToEat;
        public float TimeToEat => _TimeToEat;
    
    
        public float Eat() { 
            _FoodCount--;
            if (_FoodCount <= 0)
            {
                EnvironmentController.Instance.RemoveFood(this);
                EnvironmentController.Instance.SpawnFoodItem();
            }
            return _FoodValue;
        }
    }
}