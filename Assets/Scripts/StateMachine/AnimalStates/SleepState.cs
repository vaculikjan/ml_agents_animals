// Author: Jan Vaculik

using Agents;
using Environment;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class SleepState : IAnimalState
    {
        public AnimalState StateID => AnimalState.Sleep;

        private readonly IAnimal _animal;
        private float _sleepTime;
        private readonly float _timeToSleep;

        public SleepState(IAnimal animal, float timeToSleep)
        {
            _animal = animal;
            _timeToSleep = timeToSleep;
        }
        
        public void Enter()
        {
            _sleepTime = 0.0f;
            _animal.Acceleration = 0;
        }

        public void Execute()
        {
            _sleepTime += Time.deltaTime;

            if (!(_sleepTime >= _timeToSleep)) return;
            
            _animal.ResolveSleeping(_timeToSleep);
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
