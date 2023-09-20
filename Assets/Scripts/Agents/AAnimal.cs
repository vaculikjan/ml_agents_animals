// Author: Jan Vaculik

using System.Collections.Generic;
using UnityEngine;

namespace Agents
{
    public abstract class AAnimal : MonoBehaviour, IAnimal
    {
        [SerializeField]
        private Rigidbody _AnimalRigidbody;
        [SerializeField]
        private Food _FoodPrefab;
        
        public Food FoodPrefab => _FoodPrefab;

        [SerializeField]
        private float _FoodConsumeRadius;

        protected abstract Dictionary<AnimalStateEnum, List<AnimalStateEnum>> _validTransitions { get; }
        
        private readonly Collider[] _foodHitColliders = new Collider[10];
        protected IAnimalState CurrentState;

        public Rigidbody AnimalRigidbody => _AnimalRigidbody;

        public bool SetState(IAnimalState state)
        {
            if (!IsTransitionValid(state)) return false;

            CurrentState?.Exit();

            CurrentState = state;
            CurrentState.Enter();

            return true;
        }
        

        private bool IsTransitionValid(IAnimalState newState)
        {
            if (CurrentState == null) return true;

            return _validTransitions.TryGetValue(CurrentState.StateID, out var transition) && transition.Contains(newState.StateID) && CurrentState.CanExit();
        }

        public bool IsFoodAvailable(out Food nearestFood)
        {
            nearestFood = null;
            var nearestDistanceSqr = float.MaxValue;

            var numColliders = Physics.OverlapSphereNonAlloc(transform.position, _FoodConsumeRadius, _foodHitColliders);

            for (var i = 0; i < numColliders; i++)
            {
                var food = _foodHitColliders[i].GetComponent<Food>();

                if (!food) continue;
                var distanceSqr = (_foodHitColliders[i].transform.position - transform.position).sqrMagnitude;
                
                if (!(distanceSqr < nearestDistanceSqr)) continue;
                nearestDistanceSqr = distanceSqr;
                nearestFood = food;
            }

            return nearestFood;
        }
    }
}
