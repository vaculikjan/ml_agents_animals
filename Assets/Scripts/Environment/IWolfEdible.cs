// Author: Jan Vaculik

using Agents;

namespace Environment
{
    public interface IWolfEdible : IAttackableEdible
    {
        public IAnimal GetAnimal();
    }
}
