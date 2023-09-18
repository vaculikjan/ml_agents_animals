// Author: Jan Vaculik

using StateMachine;
using UnityEngine;

namespace Agents
{
    public interface IAnimal
    {
        public bool SetState(IAnimalState state);
    }
}
