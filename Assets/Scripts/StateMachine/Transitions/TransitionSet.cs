// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using AI;
using StateMachine.AnimalStates;
using StateMachine.Rules;
using UnityEngine;

namespace StateMachine.Transitions
{
    [Serializable]
    public class TransitionSet
    {
        [SerializeField]
        private AnimalState _State;
        
        [SerializeReference, SubclassPicker]
        private ARuleTransition[] _Transitions;
        
        public AnimalState State => _State;
        public ARuleTransition[] Transitions => _Transitions;
        
        public ARuleTransition GetTransition(IAnimal animal, IAIAnimal aiAnimal)
        {
            var validTransitions = _Transitions.Where(transition => transition.CheckTransition(animal, aiAnimal));
            return validTransitions.OrderBy(transition => transition.Priority).FirstOrDefault();
        }
    }
}
