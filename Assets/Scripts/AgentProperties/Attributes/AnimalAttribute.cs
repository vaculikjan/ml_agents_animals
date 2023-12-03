// Author: Jan Vaculik

using System;
using Unity.MLAgents;
using UnityEngine;

namespace AgentProperties.Attributes
{
    [Serializable]
    public class AnimalAttribute
    {
        [SerializeField]
        private Agent _Agent;
        [SerializeField]
        private float _MinValue;
        [SerializeField]
        private float _MaxValue;
        [SerializeField]
        private float _DefaultValue;
        [SerializeField]
        private float _RewardModifier;
        [SerializeField]
        private AnimationCurve _RewardCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float MaxValue => _MaxValue;

        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                var lastValue = _value;
                _value = Mathf.Clamp(value, _MinValue, _MaxValue);
                var reward = (_RewardCurve.Evaluate(_value) - _RewardCurve.Evaluate(lastValue)) * _RewardModifier;

                if (reward > 0)
                {
                    reward *= 50;
                }
                
                _Agent.AddReward(reward);
            }
        }
        
        public void Reset()
        {
            _value = _DefaultValue;
        }
    }
}
