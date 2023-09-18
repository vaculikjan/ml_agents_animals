// Author: Jan Vaculik

using System.Collections.Generic;
using UnityEngine;

namespace Agents
{
    public abstract class AAnimal : MonoBehaviour, IAnimal
    {
        [SerializeField]
        private Rigidbody _AnimalRigidbody;

        private readonly Dictionary<AnimalStateEnum, List<AnimalStateEnum>> _validTransitions = new()
        {
            {AnimalStateEnum.Wander, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Seek}},
            {AnimalStateEnum.Idle, new List<AnimalStateEnum> {AnimalStateEnum.Wander, AnimalStateEnum.Seek}}
        };
        
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
    }
}
