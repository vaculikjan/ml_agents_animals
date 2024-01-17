// Author: Jan Vaculik

using Agents;
using Environment;

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
        
        public void Enter() { }

        public void Execute()
        {
            _target.GetAttacked();
            _animal.SetState(new EatState(_animal, _target));
        }

        public void Exit() { }

        public bool CanExit() { return true; }

    }
}
