// Author: Jan Vaculik

using UnityEngine;
using UnityEngine.Events;

namespace AgentWrapper.Actions
{
    public abstract class AAnimalAction : MonoBehaviour
    {
        public abstract void Execute();
    }
    
    public abstract class AAnimalAction<T> : AAnimalAction
    {
        [SerializeField]
        protected T _Value;
        
        [SerializeField]
        protected UnityEvent<T> _Delegate;
        
        public override void Execute()
        {
            Execute(_Value);
        }
        
        public virtual void Execute(T value)
        {
            
        }

        public virtual void Execute(int index)
        {
            
        }
    }
}
