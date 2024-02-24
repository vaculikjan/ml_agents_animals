// Author: Jan Vaculik

using Agents;
using Environment;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class EatState : IAnimalState
    {
        public AnimalState StateID => AnimalState.Eat;

        private readonly IAnimal _animal;
        private float _eatingTime;
        private IEdible _nearestFood;

        private bool IsEating { get; set; }

        public EatState(IAnimal animal, IEdible food)
        {
            _animal = animal;
            _nearestFood = food;
        }

        public void Enter()
        {
            _eatingTime = 0.0f;
            _animal.Acceleration = 0;
        }

        public void Execute()
        {
            if (!IsEating) IsEating = true;
            
            _eatingTime += Time.deltaTime;

            if (_eatingTime <= _nearestFood.TimeToEat) return;
            
            _animal.ResolveEating(_nearestFood);
            
            IsEating = false;
            _eatingTime = 0.0f;
            _animal.SetState(new IdleState(_animal));
        }
        
        public void Exit()
        {
        }

        public bool CanExit()
        {
            return true;
        }
    }
}

