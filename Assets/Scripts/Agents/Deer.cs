// Author: Jan Vaculik

using System.Collections.Generic;
using System.Linq;
using AgentWrapper.Observations;
using UnityEngine;

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
        
        private readonly Collider[] _foodHitColliders = new Collider[10];
        
        private void Start()
        {
            //SetState(new AnimalStates.WanderState(this, _MovementSpeed,_RotationSpeed, _MinBounds, _MaxBounds));
            //SetState(new AnimalStates.SeekState(this, _MovementSpeed, _RotationSpeed, _FoodPrefab.transform.position));
        }
        
        public int Test()
        {
            return 1;
        }

        public void TestVoid()
        {
            return;
        }
        
        public void TestVoidObservation(AAnimalObservation observation)
        {
            return;
        }
        
        private void Update()
        {
            CurrentState?.Execute();
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
        }

        protected override Dictionary<AnimalStateEnum, List<AnimalStateEnum>> _validTransitions => new Dictionary<AnimalStateEnum, List<AnimalStateEnum>>
        {
            {AnimalStateEnum.Wander, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Seek}},
            {AnimalStateEnum.Idle, new List<AnimalStateEnum> {AnimalStateEnum.Wander, AnimalStateEnum.Seek, AnimalStateEnum.Eat}},
            {AnimalStateEnum.Seek, new List<AnimalStateEnum> {AnimalStateEnum.Idle, AnimalStateEnum.Eat}}
        };
        
        public Vector3[] GetPosition()
        {
            return new Vector3[] {transform.position};
        }
        
        public Food[] DetectAllFood()
        {
            var hitColliders = Physics.OverlapSphere(transform.position, _FoodConsumeRadius);

            return hitColliders.Select(hitCollider => hitCollider.GetComponent<Food>()).Where(food => food != null).ToArray();
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
        
        public void SetStateIdle()
        {
            SetState(new AnimalStates.IdleState(this));
        }
        
        public void SetStateWander()
        {
            SetState(new AnimalStates.WanderState(this, _MovementSpeed,_RotationSpeed, _MinBounds, _MaxBounds));
        }
        
        public void SetStateSeek(Vector3 targetPosition)
        {
            SetState(new AnimalStates.SeekState(this, _MovementSpeed, _RotationSpeed, targetPosition));
        }
    }
}