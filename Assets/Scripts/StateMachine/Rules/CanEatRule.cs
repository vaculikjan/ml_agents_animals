// Author: Jan Vaculik

using System;
using Agents;
using AI;

namespace StateMachine.Rules
{
    [Serializable]
    public class CanEatRule : ARule
    {
        public override bool Evaluate(IAnimal animal, IAIAnimal aiAnimal)
        {
            return aiAnimal.CanEat();
        }
    }
}
