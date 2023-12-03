// Author: Jan Vaculik

using StateMachine.AnimalStates;

namespace Agents
{
    public interface IAnimal
    {
        public bool SetState(IAnimalState state);
    }
}
