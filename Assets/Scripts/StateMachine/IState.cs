// Author: Jan Vaculik

namespace StateMachine
{
    public interface IState
    {
        public void Enter();
        public void Execute();
        public void Exit();
        public bool CanExit();
    }
}
