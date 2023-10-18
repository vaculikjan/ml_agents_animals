// Author: Jan Vaculik

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using AgentWrapper.Observations;
using Environment;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Agents
{
    public class Deer : AAnimal
    {
        [SerializeField]
        private Vector3 _MinBounds;
        [SerializeField]
        private Vector3 _MaxBounds;
        [SerializeField]
        private float _MovementSpeed;
        [SerializeField]
        private float _RotationSpeed;
        [SerializeField]
        private float _FoodConsumeRadius;
        [SerializeField]
        private float _FoodDetectionRadius;
        [SerializeField]
        private BufferSensorComponent _FoodSensor;
        
        [SerializeField]
        private AnimalAttribute _Hunger;
        [SerializeField]
        private AnimalAttribute _Curiosity;
        [SerializeField]
        private AnimalAttribute _Energy;
        
        public AnimalAttribute Hunger => _Hunger;
        public AnimalAttribute Curiosity => _Curiosity;
        public AnimalAttribute Energy => _Energy;
        
        private Collider[] _foodHitColliders;
        private Collider[] _foodDetectionColliders;
        private List<Food> _availableFood = new List<Food>();
        public List<Food> AvailableFood => _availableFood;
        
        private void Start()
        {
            //SetState(new AnimalStates.WanderState(this, _MovementSpeed,_RotationSpeed, _MinBounds, _MaxBounds));
            //SetState(new AnimalStates.SeekState(this, _MovementSpeed, _RotationSpeed, _FoodPrefab.transform.position));
        }
        
        private void FixedUpdate()
        {
            if (_availableFood.Contains(null))
            {
                _availableFood = _availableFood.Where(food => food != null).ToList();
            }
            DetectAllFood();
            CurrentState?.Execute();
            _Hunger.Value += 0.0001f;
            
            if (_Hunger.Value >= 1f)
            {
                AddReward(-10f);
                EndEpisode();
            }
            
            if (_Energy.Value <= 0f)
            {
                AddReward(-10f);
                EndEpisode();
            }

            if (CurrentState is {StateID: AnimalStateEnum.Seek} or {StateID: AnimalStateEnum.Wander})
            {
                _Energy.Value -= 0.0001f;
            }
            if (CurrentState is {StateID: AnimalStateEnum.Idle} or {StateID: AnimalStateEnum.Eat})
            {
                _Energy.Value += 0.01f;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            var corner1 = new Vector3(_MinBounds.x, 0, _MinBounds.z);
            var corner2 = new Vector3(_MaxBounds.x, 0, _MinBounds.z);
            var corner3 = new Vector3(_MaxBounds.x, 0, _MaxBounds.z);
            var corner4 = new Vector3(_MinBounds.x, 0, _MaxBounds.z);

            Gizmos.DrawLine(corner1, corner2);
            Gizmos.DrawLine(corner2, corner3);
            Gizmos.DrawLine(corner3, corner4);
            Gizmos.DrawLine(corner4, corner1);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _FoodConsumeRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _FoodDetectionRadius);
        }

        protected override Dictionary<AnimalStateEnum, List<AnimalStateEnum>> _validTransitions => new Dictionary<AnimalStateEnum, List<AnimalStateEnum>>
        {
            {AnimalStateEnum.Wander, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Seek}},
            {AnimalStateEnum.Idle, new List<AnimalStateEnum> {AnimalStateEnum.Wander, AnimalStateEnum.Seek, AnimalStateEnum.Eat, AnimalStateEnum.Idle}},
            {AnimalStateEnum.Seek, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Eat}},
            {AnimalStateEnum.Eat, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Wander, AnimalStateEnum.Seek, AnimalStateEnum.Eat}}
        };

        public override void Eat(Food food)
        {
            _Hunger.Value -= food.Eat();
        }

        public Vector3[] GetPosition()
        {
            return new Vector3[] {transform.position};
        }
        
        private void DetectAllFood()
        { 
            var foundColliders = Physics.OverlapSphere(transform.position, _FoodDetectionRadius);
            foreach (var food in foundColliders)
            {
                var foodComponent = food.GetComponent<Food>();
                if (foodComponent == null) continue;
                if (_availableFood.Contains(foodComponent)) continue;
                _Curiosity.Value += 0.1f;
                _availableFood.Add(foodComponent);
            }
        }
        
        public override bool IsFoodAvailable(out Food nearestFood)
        {
            nearestFood = null;
            var nearestDistanceSqr = float.MaxValue;

            var numColliders = Physics.OverlapSphereNonAlloc(transform.position, _FoodConsumeRadius, _foodHitColliders);

            for (var i = 0; i < numColliders; i++)
            {
                var food = _foodHitColliders[i].GetComponent<Food>();

                if (!food) continue;
                var distanceSqr = (_foodHitColliders[i].transform.position - transform.position).sqrMagnitude;
                
                if (!(distanceSqr < nearestDistanceSqr)) continue;
                nearestDistanceSqr = distanceSqr;
                nearestFood = food;
            }

            return nearestFood;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (Input.GetKeyDown(KeyCode.Q)) actionsOut.DiscreteActions.Array[0] = 1;
            else if (Input.GetKeyDown(KeyCode.W)) actionsOut.DiscreteActions.Array[0] = 2;
            else if (Input.GetKeyDown(KeyCode.E)) actionsOut.DiscreteActions.Array[0] = 4;
            else if (Input.GetKeyDown(KeyCode.A))
            {
                actionsOut.DiscreteActions.Array[0] = 3;
                actionsOut.DiscreteActions.Array[3] = 0;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                actionsOut.DiscreteActions.Array[0] = 3;
                actionsOut.DiscreteActions.Array[3] = 1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                actionsOut.DiscreteActions.Array[0] = 3;
                actionsOut.DiscreteActions.Array[3] = 2;
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(_Hunger.Value);
            sensor.AddObservation(_Energy.Value);
            sensor.AddObservation((int) CurrentState.StateID);

            foreach (var food in _foodHitColliders)
            {
                if (food == null) continue;
                var foodPosition = food.transform.position;
                var foodObservation = new float[]
                {
                    (foodPosition.x - transform.position.x),
                    (foodPosition.y - transform.position.y),
                    (foodPosition.z - transform.position.z)
                };
                _FoodSensor.AppendObservation(foodObservation);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            switch(actions.DiscreteActions[0])
            {
                case 0:
                    return;
                case 1:
                    SetState(new AnimalStates.IdleState(this));
                    break;
                case 2:
                    SetState(new AnimalStates.WanderState(this, _MovementSpeed, _RotationSpeed, _MinBounds, _MaxBounds));
                    break;
                case 3:
                    if (_availableFood.Count < actions.DiscreteActions[3] + 1)
                    {
                        AddReward(-0.1f);
                        break;
                    }
                    SetState(new AnimalStates.SeekState(this, _MovementSpeed, _RotationSpeed, _availableFood[actions.DiscreteActions[3]].transform.position));
                    break;
                case 4:
                    SetState(new AnimalStates.EatState(this));
                    break;
            }
        }

        public override void OnEpisodeBegin()
        {
            _foodHitColliders = new Collider[10];
            _foodDetectionColliders = new Collider[10];
            _availableFood = new List<Food>();
            _Hunger.Reset();
            _Curiosity.Reset();
            _Energy.Reset();
            
            MoveObjectWithinBounds();
            SetState(new AnimalStates.IdleState(this));
            EnvironmentController.Instance.ResetEnvironment();
        }
        
        private void MoveObjectWithinBounds()
        {
            var randomPosition = new Vector3(
                Random.Range(_MinBounds.x, _MaxBounds.x),
                Random.Range(_MinBounds.y, _MaxBounds.y),
                Random.Range(_MinBounds.z, _MaxBounds.z)
            );

            transform.position = randomPosition;
        }
    }
    
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

        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                var lastValue = _value;
                _value = Mathf.Clamp(value, _MinValue, _MaxValue);
                _Agent.AddReward((_RewardCurve.Evaluate(value) - _RewardCurve.Evaluate(lastValue)) * _RewardModifier);
            }
        }
        
        public void Reset()
        {
            _value = _DefaultValue;
        }
    }
}