// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Environment
{
    [Serializable]
    public class FoodManager : ASpawner<Food>
    {
        [SerializeField]
        private int _FoodCount;

        private List<Food> _foodList = new();
        
        public void ResetFood()
        {
            foreach (var food in _foodList)
            {
                Destroy(food.gameObject);
            }
            
            _foodList.Clear();
            
            for (var i = 0; i < _FoodCount; i++)
            {
                SpawnFoodItem();
            }
        }
        
        private void RemoveFood(Food food)
        {
            if (!food || food.Equals(null) || !food.gameObject) return;
            if (!_foodList.Contains(food)) return;
            
            _foodList.Remove(food);
            Destroy(food.gameObject);
        }
        
        private void SpawnFoodItem()
        {
            var food = Spawn();
            food.FoodDepleted += OnFoodDepleted;
            _foodList.Add(food);
        }
        
        #region EventHandlers
        public void OnFoodDepleted(IEdible food)
        {
            RemoveFood(food as Food);
            SpawnFoodItem();
        }
        #endregion
    }
}
