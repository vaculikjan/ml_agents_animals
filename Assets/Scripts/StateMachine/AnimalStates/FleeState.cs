using Agents;
using UnityEngine;
using Bounds = Environment.Bounds;

namespace StateMachine.AnimalStates
{
    public class FleeState : IAnimalState
    {
        private readonly IAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        private readonly Transform _threatTransform;
        private readonly Bounds _boundary;

        private readonly float _safeDistance;
        private bool _isSafe;

        public FleeState(IAnimal animal, float moveSpeed, float rotationSpeed, Transform threatTransform, float safeDistance, Bounds boundary)
        {
            _animal = animal;
            _animalTransform = animal.GetSelf().transform;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
            _threatTransform = threatTransform;
            _safeDistance = safeDistance;
            _boundary = boundary;
        }

        public AnimalState StateID => AnimalState.Flee;

        public void Enter() { _isSafe = false; }

        public void Execute()
        {
            if (_isSafe)
            {
                _animal.SetState(new IdleState(_animal));
                return;
            }

            RotateAwayFromThreat();
            CheckBounds();
            MoveAwayFromThreat();
            CheckIfSafe();
        }

        public void Exit() { _animal.AnimalRigidbody.velocity = Vector3.zero; }

        public bool CanExit() { return true; }

        private void RotateAwayFromThreat()
        {
            var directionAwayFromThreat = (_animalTransform.position - _threatTransform.position).normalized;
            var flatDirectionAwayFromThreat = new Vector3(directionAwayFromThreat.x, 0, directionAwayFromThreat.z);
            var targetRotation = Quaternion.LookRotation(flatDirectionAwayFromThreat, Vector3.up);
            var rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            _animal.AnimalRigidbody.MoveRotation(rotation);
        }

        private void MoveAwayFromThreat() { _animal.AnimalRigidbody.velocity = _animalTransform.forward * _moveSpeed; }

        private void CheckIfSafe()
        {
            var distanceToThreat = Vector3.Distance(_animalTransform.position, _threatTransform.position);
            
            if (!(distanceToThreat >= _safeDistance)) return;
            _isSafe = true;
            _animal.AnimalRigidbody.velocity = Vector3.zero;
        }
        
        private void CheckBounds()
        {
            if (_boundary.Contains(_animalTransform.position)) return;

            var position = _animalTransform.position;
            var closestPointOnBoundary = _boundary.ClosestPoint(position);
    
            var directionAwayFromThreat = (position - _threatTransform.position).normalized;
            var directionToBoundaryPoint = (closestPointOnBoundary - position).normalized;
            var blendedDirection = Vector3.Lerp(directionAwayFromThreat, directionToBoundaryPoint, 0.5f).normalized;

            var flatBlendedDirection = new Vector3(blendedDirection.x, 0, blendedDirection.z);
            var targetRotation = Quaternion.LookRotation(flatBlendedDirection, Vector3.up);
            _animalTransform.rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

    }
}
