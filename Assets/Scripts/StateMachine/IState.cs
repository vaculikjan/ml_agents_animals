// Author: Jan Vaculik

using System.Collections;

namespace StateMachine
{
    public interface IState
    {
        public void Execute();
        public IEnumerator ExitCoroutine();
        public IEnumerator EnterCoroutine();
        public bool CanExit();
    }
}
