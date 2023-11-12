// Author: Jan Vaculik

using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Agents.AnimalStates
{
    public class EatState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Eat;

        public void SetStateMask(ref IDiscreteActionMask actionMask)
        {
            if (IsEating)
            {
                for (var i = 1; i < 14; i++)
                {
                    actionMask.SetActionEnabled(0, i, false);
                }
                return;
            }
            
            for (var i = 3; i < 13; i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
        }

        private readonly AAnimal _animal;
        private float _eatingTime;
        private Food _nearestFood;

        private bool IsEating { get; set; }

        public EatState(AAnimal animal)
        {
            _animal = animal;
        }

        public void Enter()
        {
            _eatingTime = 0.0f;
            Debug.Log("Entered EatState");
        }

        public void Execute()
        {
            if (_animal.IsFoodAvailable(out _nearestFood))
            {
                if (!IsEating)
                    IsEating = true;
            }
            else
            {
                _animal.SetState(new IdleState(_animal));
                return;
            }
            
            _eatingTime += Time.deltaTime;

            if (_eatingTime <= _nearestFood.TimeToEat) return;
            
            _animal.Eat(_nearestFood);
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

