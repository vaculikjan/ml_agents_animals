// Author: Jan Vaculik

using System.Collections.Generic;

namespace StateMachine.AnimalStates
{
    public interface IAnimalState : IState
    {
        AnimalState StateID { get; }
    }

    public enum AnimalState
    {
        None,
        Idle,
        Wander,
        Seek,
        Eat,
        Pursue,
        Attack,
        Sleep,
        Flee
    }
    
    public struct AnimalStateInfo
    {
        public List<AnimalState> ValidTransitions;
        public List<int> ActionMap;
    }
}
