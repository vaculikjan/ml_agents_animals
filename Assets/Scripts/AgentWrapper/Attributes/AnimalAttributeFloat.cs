// Author: Jan Vaculik

namespace AgentWrapper.Attributes
{
    public class AnimalAttributeFloat : AAnimalAttribute<float>
    {
        public override float CalculateReward() { throw new System.NotImplementedException(); }
    }
}
