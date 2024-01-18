// Author: Jan Vaculik

using Agents;
using Environment;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class PursueState : IAnimalState
    {
        private readonly IAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;
        private readonly IAttackableEdible _target;
        private readonly Transform _targetTransform;
        private readonly float _pursuitDuration;
        private readonly float _attackRange;
        
        private float _pursuitTimer;
        private bool _hasReachedTarget;

        public PursueState(IAnimal animal, float moveSpeed, float rotationSpeed, IAttackableEdible target, float pursuitDuration, float attackRange)
        {
            _animal = animal;
            _animalTransform = animal.GetSelf().transform;
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;
            _target = target;
            _targetTransform = target.GetSelf().transform;
            _pursuitDuration = pursuitDuration;
            _attackRange = attackRange;
        }

        public AnimalState StateID => AnimalState.Pursue;
        
        public void Enter()
        {
            _hasReachedTarget = false;
            _pursuitTimer = 0f;
        }

        public void Execute()
        {
            if (_target == null || _targetTransform == null)
            {
                _animal.SetState(new IdleState(_animal));
                return;
            }
            
            if (_hasReachedTarget)
            {
                _animal.SetState(new AttackState(_animal, _target));
                return;
            }

            if (_pursuitTimer >= _pursuitDuration)
            {
                _animal.SetState(new IdleState(_animal));
                return;
            }
            
            RotateTowardsTarget();
            MoveTowardsTarget();
            CheckIfReachedTarget();

            _pursuitTimer += Time.deltaTime;
        }

        public void Exit()
        {
            _animal.AnimalRigidbody.velocity = Vector3.zero;
        }

        public bool CanExit() { return true; }

        private void RotateTowardsTarget()
        {
            var directionToTarget = (_targetTransform.position - _animalTransform.position).normalized;
            var flatDirectionToTarget = new Vector3(directionToTarget.x, 0, directionToTarget.z);
            var targetRotation = Quaternion.LookRotation(flatDirectionToTarget, Vector3.up);
            var rotation = Quaternion.RotateTowards(_animalTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

            _animal.AnimalRigidbody.MoveRotation(rotation);
        }

        private void MoveTowardsTarget() { _animal.AnimalRigidbody.velocity = _animalTransform.forward * _moveSpeed; }

        private void CheckIfReachedTarget()
        {
            var distanceToTarget = Vector3.Distance(_animalTransform.position, _targetTransform.position);
            if (!(distanceToTarget <= _attackRange)) return;
            
            _hasReachedTarget = true;
            _animal.AnimalRigidbody.velocity = Vector3.zero;
        }
    }
}
