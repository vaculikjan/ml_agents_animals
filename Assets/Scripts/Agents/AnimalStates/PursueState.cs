// Author: Jan Vaculik

using System;
using UnityEngine;

namespace Agents.AnimalStates
{
    public class PursueState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Pursue;

        private enum InternalState
        {
            Turning,
            Moving
        }

        private InternalState _currentInternalState = InternalState.Turning;

        private readonly AAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly Rigidbody _animalRigidbody;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        private readonly float _alignmentThreshold = 5.0f;

        private Transform _targetTransform;
        private bool _hasReachedTarget = false;

        public PursueState(AAnimal animal, float moveSpeed, float rotationSpeed, Transform targetTransform)
        {
            _animal = animal;
            _animalTransform = animal.transform;
            _animalRigidbody = animal.AnimalRigidbody;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
            _targetTransform = targetTransform;
        }

        public void Enter()
        {
            _hasReachedTarget = false;
        }

        public void Execute()
        {
            if (_hasReachedTarget) return;

            var targetPosition = _targetTransform.position;

            switch (_currentInternalState)
            {
                case InternalState.Turning:
                    RotateTowardsTarget(targetPosition);
                    CheckAlignment(targetPosition);
                    break;

                case InternalState.Moving:
                    RotateTowardsTarget(targetPosition);
                    MoveTowardsTarget();
                    CheckIfReachedTarget(targetPosition);
                    CheckAlignment(targetPosition);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void Exit()
        {
        }

        public bool CanExit()
        {
            return _hasReachedTarget;
        }

        private void RotateTowardsTarget(Vector3 targetPosition)
        {
            var directionToTarget = (targetPosition - _animalTransform.position).normalized;
            var flatDirectionToTarget = new Vector3(directionToTarget.x, 0, directionToTarget.z);
            var targetRotation = Quaternion.LookRotation(flatDirectionToTarget, Vector3.up);
            var rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            _animalRigidbody.MoveRotation(rotation);
        }

        private void MoveTowardsTarget()
        {
            _animalRigidbody.velocity = _animalTransform.forward * _moveSpeed;
        }

        private void CheckAlignment(Vector3 targetPosition)
        {
            var directionToTarget = (targetPosition - _animalTransform.position).normalized;
            var angle = Vector3.Angle(_animalTransform.forward, directionToTarget);

            if (_currentInternalState == InternalState.Turning && angle <= _alignmentThreshold)
            {
                _currentInternalState = InternalState.Moving;
            }
            else if (_currentInternalState == InternalState.Moving && angle > _alignmentThreshold)
            {
                _currentInternalState = InternalState.Turning;
            }
        }

        private void CheckIfReachedTarget(Vector3 targetPosition)
        {
            var distanceToTarget = Vector3.Distance(_animalTransform.position, targetPosition);
            
            if (!(distanceToTarget <= 0.1f)) return;
            _hasReachedTarget = true;
            _animalRigidbody.velocity = Vector3.zero;
        }
    }
}

