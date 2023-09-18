// Author: Jan Vaculik

using UnityEngine;

namespace Agents.AnimalStates
{
    public class IdleState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Idle;

        private readonly AAnimal _animal;
        private float _timeSpentIdle;

        public IdleState(AAnimal animal)
        {
            _animal = animal;
        }

        public void Enter()
        {
            _timeSpentIdle = 0;
        }

        public void Execute()
        {
            _timeSpentIdle += Time.deltaTime;
        }

        public void Exit()
        {
            // Maybe stop the idle animation or sound
        }

        public bool CanExit()
        {
            return true;
        }
    }
}
