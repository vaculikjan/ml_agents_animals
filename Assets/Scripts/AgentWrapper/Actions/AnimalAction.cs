// Author: Jan Vaculik

using UnityEngine;
using UnityEngine.Events;

namespace AgentWrapper.Actions
{
    public class AnimalAction : AAnimalAction
    {
        [SerializeField]
        private UnityEvent _Delegate;
        
        public override void Execute()
        {
            _Delegate?.Invoke();    
        }
    }
}
