// Author: Jan Vaculik

using System.Collections;
using Agents;
using Environment;
using Unity.MLAgents;
using UnityEngine;

namespace StateMachine.AnimalStates
{
    public class AttackState : IAnimalState
    {
        private readonly IAnimal _animal;
        private readonly IAttackableEdible _target;
        
        public AttackState(IAnimal animal, IAttackableEdible target)
        {
            _animal = animal;
            _target = target;
        }
        
        public AnimalState StateID => AnimalState.Attack;

        public void Execute()
        {
            if (_target == null)
            {
                _animal.SetState(new IdleState(_animal));
                Debug.LogWarning("Target is null, returning to idle state");
                return;
            }
            _target.GetAttacked();
            _animal.SetState(new EatState(_animal, _target));
        }
        
        public IEnumerator ExitCoroutine()
        {
            yield return null;
        }

        public IEnumerator EnterCoroutine()
        {
            if (_animal is Agent agent)
            {
                agent.AddReward(EnvironmentController.Instance.EnvironmentConfig.AttackStateReward);
            }
            yield return null;
        }

        public bool CanExit() { return true; }

    }
}
