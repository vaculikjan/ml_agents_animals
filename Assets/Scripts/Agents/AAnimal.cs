// Author: Jan Vaculik

using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

namespace Agents
{
    public abstract class AAnimal : Agent, IAnimal
    {
        [SerializeField]
        private Rigidbody _AnimalRigidbody;

        protected abstract Dictionary<AnimalStateEnum, List<AnimalStateEnum>> _validTransitions { get; }
        public abstract void Eat(Food food);
        
        protected IAnimalState CurrentState;

        public Rigidbody AnimalRigidbody => _AnimalRigidbody;

        public bool SetState(IAnimalState state)
        {
            if (!IsTransitionValid(state))
            {
                Debug.LogWarning($"Transition from {CurrentState?.StateID} to {state.StateID} is not valid!");
                return false;
            }
            
            Debug.Log($"Transition from {CurrentState?.StateID} to {state.StateID} is valid!");

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
        
        public abstract bool IsFoodAvailable(out Food food);
    }
}
