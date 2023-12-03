// Author: Jan Vaculik

using Agents;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class IdleState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Idle;

        public void SetStateMask(ref IDiscreteActionMask actionMask, int actionSize)
        {
            for (var i = 3; i < actionSize; i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
        }

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
        }

        public bool CanExit()
        {
            return true;
        }
    }
}
