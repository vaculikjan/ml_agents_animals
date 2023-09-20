// Author: Jan Vaculik

using StateMachine;

namespace Agents
{
    public interface IAnimalState : IState
    {
        AnimalStateEnum StateID { get; }
    }

    public enum AnimalStateEnum
    {
        Idle,
        Wander,
        Seek,
        Eat,
        Pursue,
    }
}
