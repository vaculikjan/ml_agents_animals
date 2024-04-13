// Author: Jan Vaculik

using System.Collections;
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
        
        public void Execute()
        {
            _sleepTime += Time.deltaTime;

            if (!(_sleepTime >= _timeToSleep)) return;
            
            _animal.ResolveSleeping(_timeToSleep);
            _animal.SetState(new IdleState(_animal));
        }

        public IEnumerator ExitCoroutine()
        {
            yield return null;
        }

        public IEnumerator EnterCoroutine()
        {
            _animal.Acceleration = 0;
            var animalRigidbody = _animal.AnimalRigidbody; 
            while (animalRigidbody.velocity.magnitude > 0.1f)
            {
                yield return new WaitForFixedUpdate();
            }
            animalRigidbody.velocity = Vector3.zero; 
        }

        public bool CanExit()
        {
            return true;
        }
    }
}
