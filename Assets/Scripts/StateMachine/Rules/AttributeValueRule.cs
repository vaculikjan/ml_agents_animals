// Author: Jan Vaculik

using System;
using AgentProperties.Attributes;
using Agents;
using AI;
using UnityEngine;

namespace StateMachine.Rules
{
    [Serializable]
    public class AttributeValueRule : ARule
    {
        [SerializeField]
        private ComparisonType _ComparisonType;
        
        [SerializeField]
        private string _Attribute;
        
        [SerializeField]
        private float _Value;

        public override bool Evaluate(IAnimal animal, IAIAnimal aiAnimal)
        {
            var attributeValue = aiAnimal.GetAttributeValue(_Attribute);

            switch (_ComparisonType)
            {
                case ComparisonType.Equal:
                    return Mathf.Approximately(attributeValue, _Value);
                case ComparisonType.Greater:
                    return attributeValue > _Value;
                case ComparisonType.Less:
                    return attributeValue < _Value;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public enum ComparisonType
    {
        Equal,
        Greater,
        Less
    }
}
