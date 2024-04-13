// Author: Jan Vaculik

using System;
using System.Collections;
using Agents;
using UnityEngine;
using Bounds = Environment.Bounds;
using Random = UnityEngine.Random;

namespace StateMachine.AnimalStates
{
    public class WanderState : IAnimalState
    {
        public AnimalState StateID => AnimalState.Wander;

        private enum InternalState
        {
            Wandering,
            ReturningToBoundary
        }
        
        private readonly IAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _rotationSpeed;
        private readonly Vector3 _boundaryMin;
        private readonly Vector3 _boundaryMax;
        
        private InternalState _currentInternalState = InternalState.Wandering;
        private float _changeDirectionCooldown;
        private Vector3 _targetDirection = Vector3.forward;
        private bool _isWithinBoundary = true;
        private Vector3 _randomTargetPosition;

        public WanderState(IAnimal animal,  float rotationSpeed, Bounds boundary)
        {
            _animal = animal;
            _animalTransform = animal.GetSelf().transform;
            _rotationSpeed = rotationSpeed;

            _boundaryMin = boundary.Min;
            _boundaryMax = boundary.Max;
        }

        public void Enter()
        {
            SetRandomTargetWithinBoundary();
        }

        public void Execute()
        {
            _animal.DetectFood();

            switch (_currentInternalState)
            {
                case InternalState.Wandering:
                    HandleBoundary();
                    if (_isWithinBoundary) HandleRandomTargetPosition();
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

        public IEnumerator ExitCoroutine()
        {
            yield return null;
        }
        
        public IEnumerator EnterCoroutine() { yield return null; }

        public bool CanExit()
        {
            return true;
        }

        private void HandleBoundary()
        {
            var nextPosition = _animalTransform.position + _targetDirection * (_animal.MovementSpeed * Time.fixedDeltaTime);

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
        private void RotateTowardsTarget()
        {
            var flatTargetDirection = new Vector3(_targetDirection.x, 0, _targetDirection.z);

            var targetRotation = Quaternion.LookRotation(flatTargetDirection, Vector3.up);
            var rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            _animal.AnimalRigidbody.MoveRotation(rotation);
        }

        private void SetVelocity()
        {
            _animal.AnimalRigidbody.velocity = _animalTransform.forward * (_animal.MovementSpeed * _animal.Acceleration);
        }
        
        private void HandleRandomTargetPosition()
        {
            if (Vector3.Distance(_animalTransform.position, _randomTargetPosition) < 1f)
            {
                SetRandomTargetWithinBoundary();
            }

            _targetDirection = (_randomTargetPosition - _animalTransform.position).normalized;
        }

        private void SetRandomTargetWithinBoundary()
        {
            _randomTargetPosition = new Vector3(
                Random.Range(_boundaryMin.x, _boundaryMax.x),
                _animalTransform.position.y,
                Random.Range(_boundaryMin.z, _boundaryMax.z)
            );
        }
    }
}
