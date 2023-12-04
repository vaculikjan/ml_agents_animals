// Author: Jan Vaculik

using Unity.MLAgents.Actuators;

namespace StateMachine.AnimalStates
{
    public interface IAnimalState : IState
    {
        AnimalStateEnum StateID { get; }
        public void SetStateMask(ref IDiscreteActionMask actionMask, int actionSize);
    }

    public enum AnimalStateEnum
    {
        None = 0,
        Idle = 1,
        Wander = 2,
        Seek = 3,
        Eat = -1,
        Pursue = 5,
        Sleep = 7,
    }
}
