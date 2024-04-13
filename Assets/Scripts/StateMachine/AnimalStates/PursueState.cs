// Author: Jan Vaculik

using System.Collections;
using Agents;
using Environment;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class PursueState : IAnimalState
    {
        private readonly IAnimal _animal;
        private readonly Transform _animalTransform;
        private readonly float _pursuitAccelMultiplier;
        private readonly float _rotationSpeed;
        private readonly IAttackableEdible _target;
        private readonly Transform _targetTransform;
        private readonly float _pursuitRange;
        private readonly float _attackRange;
        
        private bool _hasReachedTarget;

        public PursueState(IAnimal animal, float pursueAccelMultiplier, float rotationSpeed, IAttackableEdible target, float pursuitRange, float attackRange)
        {
            _animal = animal;
            _animalTransform = animal.GetSelf().transform;
            _pursuitAccelMultiplier = pursueAccelMultiplier;
            _rotationSpeed = rotationSpeed;
            _target = target;
            _targetTransform = target.GetSelf().transform;
            _pursuitRange = pursuitRange;
            _attackRange = attackRange;
        }

        public AnimalState StateID => AnimalState.Pursue;
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
            
            RotateTowardsTarget();
            MoveTowardsTarget();
            CheckIfReachedTarget();
        }

        public void Exit()
        {
            _animal.AnimalRigidbody.velocity = Vector3.zero;
        }

        public IEnumerator ExitCoroutine()
        {
            if (_animal.Equals(null)) yield break;
            _animal.MaxAcceleration = 1.0f;
            yield return null;
        }

        public IEnumerator EnterCoroutine()
        {
            _animal.MaxAcceleration = _pursuitAccelMultiplier;
            yield return null;
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

        private void MoveTowardsTarget() { _animal.AnimalRigidbody.velocity = _animalTransform.forward * (_animal.MovementSpeed * _animal.Acceleration); }

        private void CheckIfReachedTarget()
        {
            var distanceToTarget = Vector3.Distance(_animalTransform.position, _targetTransform.position);
            if (distanceToTarget > _pursuitRange)
            {
                var wolf = _animal as Wolf;
                var deer = _target as Deer;

                if (!wolf || !deer) return;
                
                if (deer.CurrentState.StateID != AnimalState.Flee) return;
                
                wolf.MarkEndChase(deer);
                wolf.SetState(new IdleState(wolf));
                return;
            }
            
            if (!(distanceToTarget <= _attackRange)) return;
            
            _hasReachedTarget = true;
            _animal.AnimalRigidbody.velocity = Vector3.zero;
        }
    }
}
