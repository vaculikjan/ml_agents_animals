// Author: Jan Vaculik

namespace Agents
{
    public class Deer : AAnimal
    {
        private void Start()
        {
            SetState(new AnimalStates.WanderState(this, 5f,90f));
        }
        
        private void Update()
        {
            CurrentState?.Execute();
        }
    }
}