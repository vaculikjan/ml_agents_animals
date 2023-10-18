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
        Idle = 0,
        Wander = 1,
        Seek = 2,
        Eat = 3,
        Pursue = 4,
    }
}
