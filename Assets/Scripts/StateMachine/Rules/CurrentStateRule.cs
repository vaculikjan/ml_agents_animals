// Author: Jan Vaculik

using System;
using Agents;
using AI;
using StateMachine.AnimalStates;
using UnityEngine;

namespace StateMachine.Rules
{
    [Serializable]
    public class CurrentStateRule : ARule
    {
        [SerializeField]
        private AnimalState _State;
        
        public override bool Evaluate(IAnimal animal, IAIAnimal aiAnimal)
        {
            return animal.CurrentState.StateID == _State;
        }
    }
}
