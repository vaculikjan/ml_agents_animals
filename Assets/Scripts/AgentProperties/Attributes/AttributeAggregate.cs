// Author: Jan Vaculik
// Created: 10.12.2023
// Copyright (c) Noxgames
// http://www.noxgames.com/

using System.Collections.Generic;
using System.Linq;

namespace AgentProperties.Attributes
{
    public class AttributeAggregate
    {
        private List<AnimalAttribute> _attributes = new ();
        
        public AttributeAggregate (IEnumerable<AnimalAttribute> attributes)
        {
            _attributes.AddRange(attributes);
        }

        public float CalculateReward()
        {
            return _attributes.Sum(attribute => attribute.EvaluateReward());
        }
    }
}
