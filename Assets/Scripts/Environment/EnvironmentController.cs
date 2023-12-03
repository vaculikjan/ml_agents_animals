// Author: Jan Vaculik

using System.Collections.Generic;
using Agents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Environment
{
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField]
        private Food _FoodPrefab;
        [SerializeField]
        private int _FoodToSpawn;
        [SerializeField]
        private Deer _Deer;
        [SerializeField]
        private Vector3 _MinBounds;
        [SerializeField]
        private Vector3 _MaxBounds;

        public static EnvironmentController Instance;
        private List<Food> _foodList = new List<Food>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void ResetEnvironment()
        {
            ResetFood();
        }
        
        public void RemoveFood(Food food)
        {
            _foodList.Remove(food);
            if (_Deer.FoodList.Contains(food))
            {
                _Deer.FoodList.Remove(food);
            }
            
            Destroy(food.gameObject);
        }

        public void SpawnFoodItem()
        {
            var food = Instantiate(_FoodPrefab, GetRandomPosition(), Quaternion.identity);
            _foodList.Add(food);
        }
        
        private void ResetFood()
        {
            foreach (var food in _foodList)
            {
                Destroy(food.gameObject);
            }
            
            _foodList = new List<Food>();
            for (var i = 0; i < _FoodToSpawn; i++)
            {
                SpawnFoodItem();
            }
        }

        private Vector3 GetRandomPosition() {
            return new Vector3(
                Random.Range(_MinBounds.x, _MaxBounds.x),
                Random.Range(_MinBounds.y, _MaxBounds.y),
                Random.Range(_MinBounds.z, _MaxBounds.z)
            );
        }
    }
}
