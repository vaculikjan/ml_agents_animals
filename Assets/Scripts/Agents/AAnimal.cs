// Author: Jan Vaculik

using System.Collections.Generic;
using AgentProperties.Attributes;
using Environment;
using StateMachine.AnimalStates;
using Unity.MLAgents;
using UnityEngine;

namespace Agents
{
    public abstract class AAnimal : Agent, IAnimal
    {
        [SerializeField]
        private Rigidbody _AnimalRigidbody;
        
        [Header("Animal Attributes")]
        [SerializeField]
        protected AnimalAttribute _Hunger;
        [SerializeField]
        protected AnimalAttribute _Curiosity;
        [SerializeField]
        protected AnimalAttribute _Energy;
        
        [SerializeField]
        protected float _EnergyRecoveryRate;
        
        public AnimalAttribute Hunger => _Hunger;
        public AnimalAttribute Curiosity => _Curiosity;
        public AnimalAttribute Energy => _Energy;

        protected abstract Dictionary<AnimalStateEnum, List<AnimalStateEnum>> ValidTransitions { get; }
        
        protected IAnimalState CurrentState;
        public Rigidbody AnimalRigidbody => _AnimalRigidbody;

        public bool SetState(IAnimalState state)
        {
            if (!IsTransitionValid(state))
            {
                Debug.LogWarning($"Transition from {CurrentState.StateID} to {state.StateID} is not valid!");
                return false;
            }
            
            CurrentState?.Exit();
            CurrentState = state;
            CurrentState.Enter();

            return true;
        }
        

        private bool IsTransitionValid(IAnimalState newState)
        {
            if (CurrentState == null) return true;

            return ValidTransitions.TryGetValue(CurrentState.StateID, out var transition) && transition.Contains(newState.StateID) && CurrentState.CanExit();
        }
        
        public abstract bool IsFoodAvailable(out Food food);
        public abstract void ResolveSleeping(float timeSlept);
    }
}
