// Author: Jan Vaculik

using System.Collections;
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

        public void Execute()
        {
            _timeSpentIdle += Time.deltaTime;
        }

        public IEnumerator ExitCoroutine()
        {
            yield return null;
        }
        public IEnumerator EnterCoroutine() { 
            
            if (_animal.Equals(null)) yield break;

            _timeSpentIdle = 0;
            _animal.Acceleration = 0;
            var animalRigidbody = _animal.AnimalRigidbody; 
            while (animalRigidbody && animalRigidbody.velocity.magnitude > 0.1f)
            {
                yield return new WaitForFixedUpdate();
            }
            if (animalRigidbody)
                animalRigidbody.velocity = Vector3.zero; 
        }

        public bool CanExit()
        {
            return true;
        }
    }
}
