// Author: Jan Vaculik

using System;
using System.Collections;
using Agents;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class SeekState : IAnimalState
    {
        private const float ALIGNMENT_THRESHOLD = 5.0f;

        private readonly IAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        private readonly Vector3 _targetPosition;

        private InternalState _currentInternalState = InternalState.Turning;
        private bool _hasReachedTarget;

        public SeekState(IAnimal animal, float rotationSpeed, Vector3 targetPosition)
        {
            _animal = animal;
            _animalTransform = animal.GetSelf().transform;
            _rotationSpeed = rotationSpeed;
            _targetPosition = targetPosition;
        }

        public AnimalState StateID => AnimalState.Seek;
        
        
        public void Execute()
        {
            if (_hasReachedTarget)
                _animal.SetState(new IdleState(_animal));

            switch (_currentInternalState)
            {
                case InternalState.Turning:
                    RotateTowardsTarget();
                    CheckAlignment();
                    break;

                case InternalState.Moving:
                    RotateTowardsTarget();
                    MoveTowardsTarget();
                    CheckIfReachedTarget();
                    CheckAlignment();
                    break;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        public IEnumerator ExitCoroutine()
        {
            yield return null;
        }
        public IEnumerator EnterCoroutine() { yield return null; }

        public bool CanExit() { return true;}

        private void RotateTowardsTarget()
        {
            var directionToTarget = (_targetPosition - _animalTransform.position).normalized;
            var flatDirectionToTarget = new Vector3(directionToTarget.x, 0, directionToTarget.z);
            var targetRotation = Quaternion.LookRotation(flatDirectionToTarget, Vector3.up);
            var rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            _animal.AnimalRigidbody.MoveRotation(rotation);
        }

        private void MoveTowardsTarget() { _animal.AnimalRigidbody.velocity = _animalTransform.forward * (_animal.MovementSpeed * _animal.Acceleration); }

        private void CheckAlignment()
        {
            var directionToTarget = (_targetPosition - _animalTransform.position).normalized;
            var angle = Vector3.Angle(_animalTransform.forward, directionToTarget);

            if (_currentInternalState == InternalState.Turning && angle <= ALIGNMENT_THRESHOLD)
                _currentInternalState = InternalState.Moving;
            else if (_currentInternalState == InternalState.Moving && angle > ALIGNMENT_THRESHOLD) _currentInternalState = InternalState.Turning;
        }

        private void CheckIfReachedTarget()
        {
            var distanceToTarget = Vector3.Distance(_animalTransform.position, _targetPosition);
            if (!(distanceToTarget <= 0.5f)) return;
            
            _hasReachedTarget = true;
            _animal.AnimalRigidbody.velocity = Vector3.zero;
            
        }

        private enum InternalState
        {
            Turning,
            Moving
        }
    }
}
