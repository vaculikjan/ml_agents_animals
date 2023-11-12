// Author: Jan Vaculik

using StateMachine;
using Unity.MLAgents.Actuators;

namespace Agents
{
    public interface IAnimalState : IState
    {
        AnimalStateEnum StateID { get; }
        public void SetStateMask(ref IDiscreteActionMask actionMask);
    }

    public enum AnimalStateEnum
    {
        None = 0,
        Idle = 1,
        Wander = 2,
        Seek = 3,
        Eat = 13,
        Pursue = 5,
    }
    
}
