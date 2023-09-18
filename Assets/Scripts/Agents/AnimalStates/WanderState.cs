// Author: Jan Vaculik

using UnityEngine;

namespace Agents.AnimalStates
{
    public class WanderState : IAnimalState
    {
        public AnimalStateEnum StateID => AnimalStateEnum.Wander;

        private readonly AAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        
        private float _changeDirectionCooldown;
        public Vector3 _targetDirection = Vector3.forward;
        
        public WanderState(AAnimal animal, float moveSpeed, float rotationSpeed)
        {
            _animal = animal;
            _animalTransform = animal.transform;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
        }

        public void Enter()
        {
            // Maybe play the wandering animation or sound
        }

        public void Execute()
        {
            HandleRandomDirectionChange();
            RotateTowardsTarget();
            SetVelocity();
        }
        
        public void Exit()
        {
            // Maybe stop the wandering animation or sound
        }

        public bool CanExit()
        {
            return true;
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
