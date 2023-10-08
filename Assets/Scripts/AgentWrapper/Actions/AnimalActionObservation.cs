// Author: Jan Vaculik

using AgentWrapper.Observations;
using UnityEngine;

namespace AgentWrapper.Actions
{
    public class AnimalActionObservationFloat : AAnimalAction<float>
    {
        [SerializeField]
        private AAnimalObservation<float> _Observation;
        
        public AAnimalObservation<float> Observation => _Observation;
        
        public override void Execute(int index)
        {
            _Delegate?.Invoke(_Observation.GetObservationVector()[index]);
        }
    
    }
}
