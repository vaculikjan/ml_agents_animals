// Author: Jan Vaculik

using System.Collections.Generic;
using UnityEngine;

namespace AgentWrapper.Observations
{
    public class AnimalObservationVector3 : AAnimalObservation<Vector3>
    {
        public override int ObservableSpaceSize => 2;
    }
}
