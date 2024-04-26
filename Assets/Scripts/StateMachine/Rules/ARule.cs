// Author: Jan Vaculik

using System;
using Agents;
using AI;

namespace StateMachine.Rules
{
    [Serializable]
    public abstract class ARule
    {
        public abstract bool Evaluate(IAnimal animal, IAIAnimal aiAnimal);
    }
}
