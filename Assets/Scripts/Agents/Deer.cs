// Author: Jan Vaculik

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Environment;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
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
        
        private Collider[] _foodHitColliders = new Collider[3];
        public Food[] AvailableFood { get; private set; } = new Food [3];
        
        private int _fixedUpdateCounter = 0;
        
        private void Start()
        {
            SetState(new AnimalStates.IdleState(this));
        }
        
        private void FixedUpdate()
        {
            DetectAllFood();
            CurrentState?.Execute();
            
            if (_Hunger.Value >= 1f)
            {
                AddReward(-10f);
                EndEpisode();
            }
            
            _fixedUpdateCounter++;
            
            if (_fixedUpdateCounter % 600 == 0)
            {
                _Hunger.Value += 0.0001f * 600;
                Debug.LogWarning($"Hunger: {_Hunger.Value}");
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

        protected override Dictionary<AnimalStateEnum, List<AnimalStateEnum>> ValidTransitions => new Dictionary<AnimalStateEnum, List<AnimalStateEnum>>
        {
            {AnimalStateEnum.Wander, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Seek}},
            {AnimalStateEnum.Idle, new List<AnimalStateEnum> {AnimalStateEnum.Wander, AnimalStateEnum.Seek, AnimalStateEnum.Eat, AnimalStateEnum.Idle}},
            {AnimalStateEnum.Seek, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Eat}},
            {AnimalStateEnum.Eat, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Wander, AnimalStateEnum.Eat}}
        };

        public override void Eat(Food food)
        {
            Debug.Log("Eating");
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
                if (AvailableFood.Contains(foodComponent)) continue;
                _Curiosity.Value += 0.1f;
                
                for (var i = 0; i < AvailableFood.Length; i++)
                {
                    if (AvailableFood[i] != null) continue;
                    AvailableFood[i] = foodComponent;
                    break;
                }
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
            sensor.AddObservation((int) CurrentState.StateID);

            foreach (var food in _foodHitColliders)
            {
                if (food == null) continue;
                var foodPosition = food.transform.localPosition;
                var foodObservation = new float[]
                {
                    (foodPosition.x - transform.localPosition.x),
                    (foodPosition.y - transform.localPosition.y),
                    (foodPosition.z - transform.localPosition.z)
                };
                _FoodSensor.AppendObservation(foodObservation);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            Debug.Log(actions.DiscreteActions[0]);
            switch(actions.DiscreteActions[0])
            {
                case (int) AnimalStateEnum.None:
                    return;
                case (int) AnimalStateEnum.Idle:
                    SetState(new AnimalStates.IdleState(this));
                    break;
                case (int) AnimalStateEnum.Wander:
                    SetState(new AnimalStates.WanderState(this, _MovementSpeed, _RotationSpeed, _MinBounds, _MaxBounds));
                    break;
                case >= 3 and <= 5:
                    SetState(new AnimalStates.SeekState(this, _MovementSpeed, _RotationSpeed, AvailableFood[actions.DiscreteActions[0] - 3].transform.position));
                    break;
                case <= 8:
                    SetState(new AnimalStates.EatState(this, AvailableFood[actions.DiscreteActions[0] - 6]));
                    break;
            }
        }

        public override void OnEpisodeBegin()
        {
            _foodHitColliders = new Collider[3];
            AvailableFood = new Food[3];
            _Hunger.Reset();
            
            MoveObjectWithinBounds();
            SetState(new AnimalStates.IdleState(this));
            EnvironmentController.Instance.ResetEnvironment();
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (CurrentState == null)
            {
                return;
            }
            CurrentState.SetStateMask(ref actionMask);

            if (CurrentState.StateID is AnimalStateEnum.Seek or AnimalStateEnum.Eat)
            {
                return;
            }
            
            for (var i = 3; i < AvailableFood.Length + 3; i++)
            {
                if (AvailableFood[i - 3] == null) continue;
                actionMask.SetActionEnabled(0, i, true);
                actionMask.SetActionEnabled(0, i + 3, true);
            }
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
                var reward = (_RewardCurve.Evaluate(value) - _RewardCurve.Evaluate(lastValue)) * _RewardModifier;
                
                if (reward > 0) reward *= 2;
                
                _Agent.AddReward(reward);
                Debug.LogWarning((reward));
            }
        }
        
        public void Reset()
        {
            _value = _DefaultValue;
        }
    }
}