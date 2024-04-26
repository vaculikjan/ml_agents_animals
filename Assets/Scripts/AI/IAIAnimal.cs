// Author: Jan Vaculik

using AgentProperties.Attributes;
using Environment;

namespace AI
{
    public interface IAIAnimal
    {
        public bool FoodAvailable { get; }
        public bool CanEat();
        
        public float GetAttributeValue(string attribute);
    }
}
