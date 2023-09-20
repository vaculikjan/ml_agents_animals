// Author: Jan Vaculik

using UnityEngine;

namespace Agents.AnimalStates
{
    public class EatState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Eat;

        private readonly AAnimal _animal;
        
        private float _eatingTime;
        private Food _nearestFood;
        private bool _isEating;
        
        public EatState(AAnimal animal)
        {
            _animal = animal;
        }

        public void Enter()
        {
            _eatingTime = 0.0f;
        }

        public void Execute()
        {
            if (_animal.IsFoodAvailable(out _nearestFood) && !_isEating)
            {
                _isEating = true;
            }
            else
            {
                _animal.SetState(new IdleState(_animal));
            }

            if (!_isEating) return;
            
            _eatingTime += Time.deltaTime;
                
            if (_eatingTime >= _nearestFood.TimeToEat)
            {
                _animal.SetState(new IdleState(_animal));
            }
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

