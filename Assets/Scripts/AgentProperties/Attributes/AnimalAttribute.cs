// Author: Jan Vaculik

using System;
using Unity.MLAgents;
using UnityEngine;

namespace AgentProperties.Attributes
{
    [Serializable]
    public class AnimalAttribute
    {
        [Header("Attribute")]
        [SerializeField]
        private Agent _Agent;
        [SerializeField]
        private float _MinValue = 0f;
        [SerializeField]
        private float _MaxValue = 1f;
        [SerializeField]
        private float _DefaultValue = 1f;
        
        [Header("Reward")]
        [SerializeField]
        private float _RewardModifier = 1f;
        [SerializeField]
        private float _PositiveMultiplier = 1f;
        [SerializeField]
        private AnimationCurve _RewardCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField]
        private AnimationCurve _PositiveMultiplierCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [SerializeField]
        private AnimationCurve _NegativeMultiplierCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        public float MinValue => _MinValue;
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
                    reward *= _PositiveMultiplierCurve.Evaluate(lastValue) * _PositiveMultiplier;
                }
                else
                {
                    reward *= _NegativeMultiplierCurve.Evaluate(lastValue);
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
