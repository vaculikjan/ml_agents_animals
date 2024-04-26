// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using AI;
using StateMachine.Rules;
using UnityEngine;
using UnityEngine.Serialization;

namespace StateMachine.Transitions
{
    [Serializable]
    public abstract class ARuleTransition
    {
        [SerializeReference, SubclassPicker]
        protected ARule[] _Rules;
        
        [SerializeField]
        private int _Priority;
        
        public virtual bool CheckTransition(IAnimal animal, IAIAnimal aiAnimal)
        {
            return _Rules.All(rule => rule.Evaluate(animal, aiAnimal));
        }

        public abstract void MakeTransition(IAnimal animal, IAIAnimal aiAnimal);
        
        public int Priority => _Priority;
    }
}
