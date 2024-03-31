using Agents;
using Environment;
using Unity.MLAgents;
using UnityEngine;
using Bounds = Environment.Bounds;

namespace StateMachine.AnimalStates
{
    public class FleeState : IAnimalState
    {
        private readonly IAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _accelMultiplier;
        private readonly float _rotationSpeed;
        private readonly Transform _threatTransform;
        private readonly Bounds _boundary;

        private readonly float _safeDistance;
        private bool _isSafe;

        public FleeState(IAnimal animal, float accelMultiplier, float rotationSpeed, Transform threatTransform, float safeDistance, Bounds boundary)
        {
            _animal = animal;
            _animalTransform = animal.GetSelf().transform;
            _accelMultiplier = accelMultiplier;
            _rotationSpeed = rotationSpeed;
            _threatTransform = threatTransform;
            _safeDistance = safeDistance;
            _boundary = boundary;
        }

        public AnimalState StateID => AnimalState.Flee;

        public void Enter()
        {
            _isSafe = false; 
            _animal.MaxAcceleration = _accelMultiplier;
        }

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

        public void Exit()
        {
            _animal.AnimalRigidbody.velocity = Vector3.zero; 
            _animal.MaxAcceleration = 1;
            
            if (_animal is Agent agent)
            {
                agent.AddReward(EnvironmentController.Instance.EnvironmentConfig.FleeStateReward);
            }
        }

        public bool CanExit() { return true; }

        private void RotateAwayFromThreat()
        {
            if (!_threatTransform || _threatTransform.Equals(null))
            {
                _isSafe = true;
                return;
            }
            
            var directionAwayFromThreat = (_animalTransform.position - _threatTransform.position).normalized;
            var flatDirectionAwayFromThreat = new Vector3(directionAwayFromThreat.x, 0, directionAwayFromThreat.z);
            var targetRotation = Quaternion.LookRotation(flatDirectionAwayFromThreat, Vector3.up);
            var rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            _animal.AnimalRigidbody.MoveRotation(rotation);
        }

        private void MoveAwayFromThreat() { _animal.AnimalRigidbody.velocity = _animalTransform.forward * (_animal.MovementSpeed * _animal.Acceleration); }

        private void CheckIfSafe()
        {
            if (!_threatTransform || _threatTransform.Equals(null))
            {
                _isSafe = true;
                return;
            }
            
            var distanceToThreat = Vector3.Distance(_animalTransform.position, _threatTransform.position);
            
            if (!(distanceToThreat >= _safeDistance)) return;
            
            _isSafe = true;
            
            _animal.AnimalRigidbody.velocity = Vector3.zero;
            var wolf = _threatTransform.GetComponent<Wolf>();
            
            if (wolf)
            {
                wolf.MarkEndChase(_animal);
            }
        }
        
        private void CheckBounds()
        {
            if (_boundary.Contains(_animalTransform.position) || !_threatTransform || _threatTransform.Equals(null)) return;

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
