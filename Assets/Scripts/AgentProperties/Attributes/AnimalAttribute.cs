// Author: Jan Vaculik

using System;
using UnityEditor;
using UnityEngine;

namespace AgentProperties.Attributes
{
    [Serializable]
    public class AnimalAttribute
    {
        [Header("Attribute")]
        [SerializeField]
        private float _MinValue = 0f;
        [SerializeField]
        private float _MaxValue = 1f;
        [SerializeField]
        private float _DefaultValue = 1f;
        
        [Header("Reward")]
        [SerializeField]
        private AnimationCurve _RewardCurve = AnimationCurve.Linear(0f, -1f, 1f, 1f);
        [SerializeField]
        private bool _InvertReward;
        [Range(0f, 10f)]
        [SerializeField]
        private float _RewardMultiplier = 1f;

        public float MinValue => _MinValue;
        public float MaxValue => _MaxValue;

        private float _value;
        public float Value
        {
            get => _value;
            set => _value = Mathf.Clamp(value, _MinValue, _MaxValue);
        }
        
        public float EvaluateReward()
        {
            var reward = _InvertReward ? _RewardCurve.Evaluate(1-_value) : _RewardCurve.Evaluate(_value);
            return reward * _RewardMultiplier;
        }
        
        public void Reset()
        {
            _value = _DefaultValue;
        }

        public void SetCurveFromArray(float[] keys)
        {
            for (var i = 0; i < keys.Length; i++)
            {
                _RewardCurve.keys[i].value = keys[i];
            }
        }
    }
}
