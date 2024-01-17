// Author: Jan Vaculik

using AgentProperties.Attributes;
using Environment;
using StateMachine.AnimalStates;
using UnityEngine;

namespace Agents
{
    public interface IAnimal
    {
        public bool SetState(IAnimalState state);
        public Rigidbody AnimalRigidbody { get; }
        public AnimalAttribute Hunger { get; }
        public GameObject GetSelf();
        public void ResolveSleeping(float timeSlept);
        public void ResolveEating(IEdible food);
        public void DetectFood();
    }
}
