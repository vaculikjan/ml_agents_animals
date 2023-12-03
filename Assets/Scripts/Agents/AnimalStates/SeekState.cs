// Author: Jan Vaculik

using System;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Agents.AnimalStates
{
    public class SeekState : IAnimalState
    {
        private const float ALIGNMENT_THRESHOLD = 5.0f;

        private readonly AAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        private readonly Vector3 _targetPosition;

        private InternalState _currentInternalState = InternalState.Turning;
        private bool _hasReachedTarget;

        public SeekState(AAnimal animal, float moveSpeed, float rotationSpeed, Vector3 targetPosition)
        {
            _animal = animal;
            _animalTransform = animal.transform;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
            _targetPosition = targetPosition;
        }

        public AnimalStateEnum StateID => AnimalStateEnum.Seek;

        public void SetStateMask(ref IDiscreteActionMask actionMask, int actionSize)
        {
            actionMask.SetActionEnabled(0, (int) AnimalStateEnum.Wander, false);
            for (var i = 3; i < actionSize; i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
        }

        public void Enter() { _hasReachedTarget = false; }

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

        public void Exit()
        {
            _animal.AnimalRigidbody.velocity = Vector3.zero;
        }

        public bool CanExit() { return true;}

        private void RotateTowardsTarget()
        {
            var directionToTarget = (_targetPosition - _animalTransform.position).normalized;
            var flatDirectionToTarget = new Vector3(directionToTarget.x, 0, directionToTarget.z);
            var targetRotation = Quaternion.LookRotation(flatDirectionToTarget, Vector3.up);
            var rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            _animal.AnimalRigidbody.MoveRotation(rotation);
        }

        private void MoveTowardsTarget() { _animal.AnimalRigidbody.velocity = _animalTransform.forward * _moveSpeed; }

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
