// Author: Jan Vaculik

using Agents;
using Environment;
using Unity.MLAgents;

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

        public void Enter()
        {
            _animal.Acceleration = 0;
            if (_animal is Agent agent)
            {
                agent.AddReward(EnvironmentController.Instance.EnvironmentConfig.AttackStateReward);
            }
        }

        public void Execute()
        {
            if (_target == null)
            {
                _animal.SetState(new IdleState(_animal));
                return;
            }
            _target.GetAttacked();
            _animal.SetState(new EatState(_animal, _target));
        }

        public void Exit() { }

        public bool CanExit() { return true; }

    }
}
