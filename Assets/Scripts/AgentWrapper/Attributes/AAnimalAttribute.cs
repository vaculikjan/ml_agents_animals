// Author: Jan Vaculik

using System;
using UnityEngine;

namespace AgentWrapper.Attributes
{
    public abstract class AAnimalAttribute : MonoBehaviour
    {
        [SerializeField]
        private string _Name = "Attribute";
        public string Name => _Name;

        [SerializeField]
        protected AnimationCurve _ImpactCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField]
        protected float _ImpactMultiplier = 1f;

        public abstract float CalculateReward();
    }
    
    public abstract class AAnimalAttribute<T> : AAnimalAttribute
    {
        private T _currentValue;
        
        public T CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                OnValueChanged?.Invoke(_currentValue);
            }
        }

        public Action<T> OnValueChanged;
    }
}
