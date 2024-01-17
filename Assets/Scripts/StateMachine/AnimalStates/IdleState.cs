// Author: Jan Vaculik

using Agents;
using Environment;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class IdleState : IAnimalState
    {
        public AnimalState StateID => AnimalState.Idle;

        private readonly IAnimal _animal;
        private float _timeSpentIdle;

        public IdleState(IAnimal animal)
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
