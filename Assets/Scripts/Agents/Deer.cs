// Author: Jan Vaculik

using System;
using System.Collections.Generic;
using AgentProperties.Attributes;
using Environment;
using StateMachine.AnimalStates;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Agents
{
    public class Deer : AAnimal
    {
        [Header("Boundaries")]
        [SerializeField]
        private Vector3 _MinBounds;
        [SerializeField]
        private Vector3 _MaxBounds;
        [SerializeField]
        
        [Header("Movement variables")]
        private float _MovementSpeed;
        [SerializeField]
        private float _RotationSpeed;
        [SerializeField]
        
        [Header("Food variables")]
        private float _FoodConsumeRadius;
        [SerializeField]
        private float _FoodDetectionRadius;
        [SerializeField]
        private BufferSensorComponent _FoodSensor;

        [Header("Attribute training variables")]
        [SerializeField]
        private float _HungerPerSecond;
        [SerializeField]
        private float _EnergyPerSecond;
        [SerializeField]
        private float _EnergyRegenPerSecond;
        [SerializeField]
        private float _TimeToSleep;
        [SerializeField]
        private float _CuriosityPerSecond;
        [SerializeField]
        private float _CuriosityDecayPerSecond;
        
        [Header("Miscellaneous")]
        [SerializeField]
        private float _MaxLifeSpan = 180f;
        [SerializeField]
        private float _MinLifeSpan = 120f;
        
        private float _currentLifeSpan;
        private float _timeLiving;
        
        private Collider[] _foodHitColliders = new Collider[3];
        private List<Food> _foodList = new();
        private Food[] _availableFood = new Food [3];
        private Food _foodToEat;
        private int _fixedUpdateCounter;
        private bool _resting => CurrentState?.StateID == AnimalStateEnum.Sleep;

        private AttributeAggregate _attributeAggregate;

        public List <Food> FoodList => _foodList;
        public int _MaxDiscreteStates = 8;
        
        private void Start()
        {
            SetState(new IdleState(this));
            _attributeAggregate = new AttributeAggregate(new List<AnimalAttribute> {_Hunger, _Energy, _Curiosity});
        }
        
        private void FixedUpdate()
        {
            // food detection
            if (CurrentState?.StateID == AnimalStateEnum.Wander)
            {
                DetectAllFood();
            }
            
            CalculateClosestFood();
            CurrentState?.Execute();
            
            _fixedUpdateCounter++;
            
            // lifespan handling
            HandleLifeSpan();
            
            // attribute handling
            HandleHunger();
            HandleEnergy();
            HandleCuriosity();
            
            // reward distribution
            if (_fixedUpdateCounter % 50 != 0) return;
            var reward = _attributeAggregate.CalculateReward();
            AddReward(reward);
        }

        private void HandleHunger()
        {
            
            if (_fixedUpdateCounter % 50 == 0)
            {
                _Hunger.Value += _HungerPerSecond * (_resting ? 0.5f : 1f);
            }

            if (!(_Hunger.Value >= _Hunger.MaxValue)) return;
            
            SetReward(-1000f);
            EndEpisode();
        }

        private void HandleEnergy()
        {
            
            if (_fixedUpdateCounter % 50 == 0)
            {
                _Energy.Value += _EnergyPerSecond;
            }
            

            if (!(_Energy.Value <= _Energy.MinValue)) return;
            
            SetReward(-1000f);
            EndEpisode();
        }
        
        private void HandleCuriosity()
        {
            if (CurrentState?.StateID == AnimalStateEnum.Wander)
            {
                _Curiosity.Value += _CuriosityDecayPerSecond * Time.fixedDeltaTime;
                return;
            }
            
            if (_fixedUpdateCounter % 50 != 0) return;
            {
                _Curiosity.Value += _CuriosityPerSecond;
            }
        }
        
        private void HandleLifeSpan()
        {
            _timeLiving += Time.fixedDeltaTime;
            if (_timeLiving < _currentLifeSpan) return;
            
            EndEpisode();
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
            {AnimalStateEnum.Wander, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Seek, AnimalStateEnum.Sleep, AnimalStateEnum.Eat}},
            {AnimalStateEnum.Idle, new List<AnimalStateEnum> { AnimalStateEnum.Idle, AnimalStateEnum.Wander, AnimalStateEnum.Seek, AnimalStateEnum.Eat, AnimalStateEnum.Sleep}},
            {AnimalStateEnum.Seek, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Eat}},
            {AnimalStateEnum.Eat, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Wander, AnimalStateEnum.Eat}},
            {AnimalStateEnum.Sleep, new List<AnimalStateEnum> {AnimalStateEnum.Idle}}
        };
        
        private void DetectAllFood()
        { 
            var foundColliders = Physics.OverlapSphere(transform.position, _FoodDetectionRadius);
            foreach (var food in foundColliders)
            {
                var foodComponent = food.GetComponent<Food>();
                if (foodComponent == null) continue;
                if (_foodList.Contains(foodComponent)) continue;
                    AddReward(5f);
                _foodList.Add(foodComponent);
            }
        }

        private void CalculateClosestFood()
        {
            _foodList.Sort((x, y) => Vector3.Distance(transform.position, x.transform.position).CompareTo(Vector3.Distance(transform.position, y.transform.position)));
            
            for (var i = 0; i < _availableFood.Length; i++)
            {
                if (i >= _foodList.Count)
                {
                    _availableFood[i] = null;
                    continue;
                }
                _availableFood[i] = _foodList[i];
            }
        }

        private bool CanEatAvailableFood(out Food food)
        {
            food = null;
            
            foreach (var x in _availableFood)
            {
                if (x == null) continue;
                if (!(Vector3.Distance(transform.position, x.transform.position) <= _FoodConsumeRadius)) continue;
                food = x;
                return true;
            }

            return false;
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

        public override void ResolveSleeping(float timeSlept)
        {
            Energy.Value += timeSlept * _EnergyRegenPerSecond;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (Input.GetKey(KeyCode.Q)) actionsOut.DiscreteActions.Array[0] = (int) AnimalStateEnum.Idle;
            else if (Input.GetKey(KeyCode.W)) actionsOut.DiscreteActions.Array[0] = (int) AnimalStateEnum.Wander;
            else if (Input.GetKey(KeyCode.E)) actionsOut.DiscreteActions.Array[0] = 3;
            else if (Input.GetKey(KeyCode.R)) actionsOut.DiscreteActions.Array[0] = 6;
            else if (Input.GetKey(KeyCode.T)) actionsOut.DiscreteActions.Array[0] = (int) AnimalStateEnum.Sleep;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_Hunger.Value);
            sensor.AddObservation(_Energy.Value);
            sensor.AddObservation(_Curiosity.Value);
            sensor.AddObservation((int) CurrentState.StateID);

            foreach (var food in _availableFood)
            {
                if (food == null) continue;
                var foodPosition = food.transform.localPosition;
                var distanceToFood = Vector3.Distance(transform.localPosition, foodPosition);
                var foodObservation = new float[]
                {
                    distanceToFood
                };
                _FoodSensor.AppendObservation(foodObservation);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            switch(actions.DiscreteActions[0])
            {
                case (int) AnimalStateEnum.None:
                    return;
                case (int) AnimalStateEnum.Idle:
                    SetState(new IdleState(this));
                    break;
                case (int) AnimalStateEnum.Wander:
                    SetState(new WanderState(this, _MovementSpeed, _RotationSpeed, _MinBounds, _MaxBounds));
                    break;
                case >= 3 and <= 5:
                    SetState(new SeekState(this, _MovementSpeed, _RotationSpeed, _availableFood[actions.DiscreteActions[0] - 3].transform.position));
                    break;
                case 6:
                    SetState(new EatState(this, _foodToEat));
                    break;
                case (int) AnimalStateEnum.Sleep:
                    SetState(new SleepState(this, _TimeToSleep));
                    break;
            }
        }

        public override void OnEpisodeBegin()
        {
            // reset food variables
            _foodList = new List<Food>();
            _foodHitColliders = new Collider[3];
            _availableFood = new Food[3];
            
            // reset attributes
            _Hunger.Reset();
            _Energy.Reset();
            _Curiosity.Reset();
            
            // reset position
            MoveObjectWithinBounds();
            
            // reset lifespan
            _currentLifeSpan = Random.Range(_MinLifeSpan, _MaxLifeSpan);
            _timeLiving = 0f;
            
            // reset state
            SetState(new IdleState(this));
            
            // reset environment
            EnvironmentController.Instance.ResetEnvironment();
            Debug.Log(GetCumulativeReward());
        }
        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (CurrentState == null)
            {
                return;
            }
            CurrentState.SetStateMask(ref actionMask, _MaxDiscreteStates);
            
            if (_Energy.Value > 0.5f)
            {
                actionMask.SetActionEnabled(0, 7, false);
            }
            
            if (CurrentState.StateID is AnimalStateEnum.Sleep)
            {
                return;
            }
            
            if (CanEatAvailableFood(out _foodToEat))
            {
                actionMask.SetActionEnabled(0, 6, true);
            }

            if (CurrentState.StateID is AnimalStateEnum.Eat or AnimalStateEnum.Seek)
            {
                return;
            }
            
            for (var i = 3; i < _availableFood.Length + 3; i++)
            {
                if (_availableFood[i - 3] == null) continue;
                actionMask.SetActionEnabled(0, i, true);
            }
        }

        private void MoveObjectWithinBounds()
        {
            var randomPosition = new Vector3(
                Random.Range(_MinBounds.x/2, _MaxBounds.x/2),
                Random.Range(_MinBounds.y/2, _MaxBounds.y/2),
                Random.Range(_MinBounds.z/2, _MaxBounds.z/2)
            );

            transform.position = randomPosition;
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }
    }
}