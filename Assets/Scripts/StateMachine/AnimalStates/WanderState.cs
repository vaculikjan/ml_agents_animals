// Author: Jan Vaculik

using System;
using Agents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StateMachine.AnimalStates
{
    public class WanderState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Wander;

        public void SetStateMask(ref IDiscreteActionMask actionMask, int actionSize)
        {
            actionMask.SetActionEnabled(0, (int) AnimalStateEnum.Wander, false);
            for (var i = 3; i < actionSize; i++)
            {
                actionMask.SetActionEnabled(0, i, false);
            }
        }

        private enum InternalState
        {
            Wandering,
            ReturningToBoundary
        }
        
        private readonly AAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        private readonly Vector3 _boundaryMin;
        private readonly Vector3 _boundaryMax;
        
        private InternalState _currentInternalState = InternalState.Wandering;
        private float _changeDirectionCooldown;
        private Vector3 _targetDirection = Vector3.forward;
        private bool _isWithinBoundary = true;

        public WanderState(AAnimal animal, float moveSpeed, float rotationSpeed, Vector3 boundaryMin, Vector3 boundaryMax)
        {
            _animal = animal;
            _animalTransform = animal.transform;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;

            _boundaryMin = boundaryMin;
            _boundaryMax = boundaryMax;
        }

        public void Enter()
        {
        }

        public void Execute()
        {
            switch (_currentInternalState)
            {
                case InternalState.Wandering:
                    HandleBoundary();
                    if (_isWithinBoundary) HandleRandomDirectionChange();
                    RotateTowardsTarget();
                    SetVelocity();
                    break;

                case InternalState.ReturningToBoundary:
                    HandleBoundary();
                    RotateTowardsTarget();
                    SetVelocity();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void Exit()
        {
            _animal.AnimalRigidbody.velocity = Vector3.zero;
        }

        public bool CanExit()
        {
            return true;
        }

        private void HandleBoundary()
        {
            var nextPosition = _animalTransform.position + _targetDirection * (_moveSpeed * Time.deltaTime);

            if (nextPosition.x < _boundaryMin.x || nextPosition.x > _boundaryMax.x ||
                nextPosition.z < _boundaryMin.z || nextPosition.z > _boundaryMax.z)
            {
                _isWithinBoundary = false;
                _currentInternalState = InternalState.ReturningToBoundary;

                var center = (_boundaryMin + _boundaryMax) / 2;
                _targetDirection = (center - _animalTransform.position).normalized;
            }
            else
            {
                _isWithinBoundary = true;
                _currentInternalState = InternalState.Wandering;
            }
        }

        private void HandleRandomDirectionChange()
        {
            _changeDirectionCooldown -= Time.deltaTime;

            if (_changeDirectionCooldown > 0) return;

            var angleChange = Random.Range(-90f, 90f);
            var rotation = Quaternion.AngleAxis(angleChange, Vector3.up);
            _targetDirection = rotation * _targetDirection;

            _targetDirection.Normalize();
            _changeDirectionCooldown = Random.Range(1f, 5f);
        }

        private void RotateTowardsTarget()
        {
            var flatTargetDirection = new Vector3(_targetDirection.x, 0, _targetDirection.z);

            var targetRotation = Quaternion.LookRotation(flatTargetDirection, Vector3.up);
            var rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            _animal.AnimalRigidbody.MoveRotation(rotation);
        }

        private void SetVelocity()
        {
            _animal.AnimalRigidbody.velocity = _animalTransform.forward * _moveSpeed;
        }
    }
}
