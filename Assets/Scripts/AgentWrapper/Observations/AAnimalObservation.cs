// Author: Jan Vaculik

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AgentWrapper.Observations
{
    public abstract class AAnimalObservation : MonoBehaviour
    {
        [SerializeField]
        private string _Name = "Observation";
        public string Name => _Name;
        
        [SerializeField]
        protected int _ObservationVectorLength = 1;
        public int ObservationVectorLength => _ObservationVectorLength;
        
        public abstract int ObservableSpaceSize { get; }
    }
    
    public abstract class AAnimalObservation<T> : AAnimalObservation
    {
        [SerializeField]
        protected SerializableCallback<T[]> _UpdateObservationCallback;

        public List<T> GetObservationVector()
        {
            return _UpdateObservationCallback.Invoke().ToList();
        }
    }
}
