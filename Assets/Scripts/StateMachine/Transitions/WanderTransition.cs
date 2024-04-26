// Author: Jan Vaculik

using System;
using Agents;
using AI;
using Environment;
using StateMachine.AnimalStates;

namespace StateMachine.Transitions
{
    [Serializable]
    public class WanderTransition : ARuleTransition
    {
        public override void MakeTransition(IAnimal animal, IAIAnimal aiAnimal) { 
            animal.SetState(new WanderState(animal, animal.RotationSpeed, AIEnvironmentController.Instance.ArenaBounds));
        }
        
        public WanderTransition() {
           
        }
    }
}
