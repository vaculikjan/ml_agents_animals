// Author: Jan Vaculik

using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Agents.AnimalStates
{
    public class IdleState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Idle;

        public void SetStateMask(ref IDiscreteActionMask actionMask)
        {
            for (var i = 3; i < 13; i++)
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
