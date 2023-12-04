// Author: Jan Vaculik

using Agents;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class SleepState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Sleep;

        private readonly AAnimal _animal;
        private float _sleepTime;
        private readonly float _timeToSleep;
        private bool _isSleeping;

        public SleepState(AAnimal animal, float timeToSleep)
        {
            _animal = animal;
            _timeToSleep = timeToSleep;
        }
        
        public static void MaskState(ref IDiscreteActionMask actionMask, bool isEnabled)
        {
            actionMask.SetActionEnabled(0, 7, isEnabled);
        }

        public void SetStateMask(ref IDiscreteActionMask actionMask, int actionSize)
        {
            for (var i = 1; i < actionSize; i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
        }

        public void Enter()
        {
            _sleepTime = 0.0f;
            _isSleeping = true;
        }

        public void Execute()
        {
            Debug.Log("Sleeping");
            _sleepTime += Time.deltaTime;

            if (!(_sleepTime >= _timeToSleep)) return;
            
            _isSleeping = false;
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
