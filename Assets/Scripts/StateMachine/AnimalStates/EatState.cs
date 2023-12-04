// Author: Jan Vaculik

using Agents;
using Environment;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class EatState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Eat;

        public void SetStateMask(ref IDiscreteActionMask actionMask, int actionSize)
        {
            if (IsEating)
            {
                for (var i = 1; i < actionSize; i++)
                {
                    actionMask.SetActionEnabled(0, i, false);
                }
            }
        }

        private readonly AAnimal _animal;
        private float _eatingTime;
        private Food _nearestFood;

        private bool IsEating { get; set; }

        public EatState(AAnimal animal, Food food)
        {
            _animal = animal;
            _nearestFood = food;
        }

        public void Enter()
        {
            _eatingTime = 0.0f;
        }

        public void Execute()
        {
            Debug.Log("Eating");
            if (!IsEating) IsEating = true;
            
            _eatingTime += Time.deltaTime;

            if (_eatingTime <= _nearestFood.TimeToEat) return;
            
            _animal.Hunger.Value -= _nearestFood.Eat();
            
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

