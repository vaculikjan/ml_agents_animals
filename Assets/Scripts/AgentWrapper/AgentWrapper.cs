// Author: Jan Vaculik

using System.Linq;
using Agents;
using AgentWrapper.Actions;
using AgentWrapper.Attributes;
using AgentWrapper.Observations;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace AgentWrapper
{
    public class AgentWrapper : Agent
    {
        [SerializeField]
        private AAnimal _Animal;

        [SerializeField]
        private AAnimalAttribute[] _AnimalAttributes;
        
        [SerializeField]
        private AAnimalObservation[] _AnimalObservations;
        
        [SerializeField]
        private AAnimalAction[] _AnimalActions;
        
        private BehaviorParameters _behaviorParameters;
        private void OnValidate()
        {
            _behaviorParameters = GetComponent<BehaviorParameters>();
        }

        private void Start()
        {
            var observationSpaceSize = 0;
            foreach (var observation in _AnimalObservations)
            {
                if (observation.ObservationVectorLength <= 1)
                {
                    switch (observation)
                    {
                        case AAnimalObservation<float> floatObservation: observationSpaceSize += floatObservation.ObservableSpaceSize;
                            break;
                        case AAnimalObservation<Vector3> vector3Observation: observationSpaceSize += vector3Observation.ObservableSpaceSize;
                            break;
                    }
                }
                
                var bufferSensor = gameObject.AddComponent<BufferSensorComponent>();
                bufferSensor.SensorName = observation.Name;
                bufferSensor.ObservableSize = observation.ObservableSpaceSize;
                bufferSensor.MaxNumObservables = observation.ObservationVectorLength;
            }
            
            _behaviorParameters.BrainParameters.VectorObservationSize = observationSpaceSize;

            var numberOfActions = _AnimalActions.Length;
            var branchSizes = new int[numberOfActions + 1];
            branchSizes[0] = numberOfActions;
            
            for (var i = 0; i < numberOfActions; i++)
            {
                switch (_AnimalActions[i])
                {
                    case AnimalActionObservationFloat floatAction:
                        branchSizes[i + 1] = floatAction.Observation.ObservableSpaceSize;
                        break;
                }
            }

            var brainParametersActionSpec = _behaviorParameters.BrainParameters.ActionSpec;
            
            brainParametersActionSpec.BranchSizes = new [] {numberOfActions};
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            foreach (var attribute in _AnimalAttributes)
            {
                switch (attribute)
                {
                    case AAnimalAttribute<float> floatAttribute: sensor.AddObservation(floatAttribute.CurrentValue);
                        break;
                    case AAnimalAttribute<Vector3> vector3Attribute: sensor.AddObservation(vector3Attribute.CurrentValue);
                        break;
                }
            }
            
            foreach (var observation in _AnimalObservations)
            {
                if (observation.ObservationVectorLength == 1)
                {
                    switch (observation)
                    {
                        case AAnimalObservation<float> floatObservation: 
                            sensor.AddObservation(floatObservation.GetObservationVector()[0]);
                            break;
                        case AAnimalObservation<Vector3> vector3Observation: 
                            sensor.AddObservation(vector3Observation.GetObservationVector()[0]);
                            break;
                    }
                }
                
                else if (observation.ObservationVectorLength > 1)
                {
                    switch (observation)
                    {
                        case AAnimalObservation<float> floatObservation: 
                            var bufferSensor = GetComponents<BufferSensorComponent>().First(x => x.SensorName == floatObservation.Name);
                            AppendObservationsToBufferSensor(floatObservation, bufferSensor);
                            break;
                        case AAnimalObservation<Vector3> vector3Observation: 
                            var bufferSensor2 = GetComponents<BufferSensorComponent>().First(x => x.SensorName == vector3Observation.Name);
                            AppendObservationsToBufferSensor(vector3Observation, bufferSensor2);
                            break;
                    }
                }
            }
        }
        
        public override void OnActionReceived(ActionBuffers actions)
        {
            if (_AnimalActions[actions.DiscreteActions[0]] is AnimalActionObservationFloat floatAction)
            {
                floatAction.Execute(actions.DiscreteActions[actions.DiscreteActions[0]]);
            }
            else if (_AnimalActions[actions.DiscreteActions[0]] is AnimalAction action)
            {
                action.Execute();
            }
        }
        
        private void AppendObservationsToBufferSensor<T>(AAnimalObservation<T> observation, BufferSensorComponent bufferSensor)
        {
            switch (observation)
            {
                case AAnimalObservation<float> floatObservation:
                    foreach (var value in floatObservation.GetObservationVector())
                    {
                        bufferSensor.AppendObservation(new [] {value});
                    }
                    break;
                case AAnimalObservation<Vector3> vector3Observation:
                    foreach (var value in vector3Observation.GetObservationVector())
                    {
                        var animalPosition = _Animal.transform.position;
                        bufferSensor.AppendObservation(new [] {animalPosition.x - value.x, animalPosition.z - value.z});
                    }
                    break;
            }
        }
    }
}
