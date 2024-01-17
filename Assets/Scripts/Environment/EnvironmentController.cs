// Author: Jan Vaculik

using Agents;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment
{
    public class EnvironmentController : MonoBehaviour
    {
        [Header("Arena")]
        [SerializeField]
        private Bounds _ArenaBounds;
        
        [Header("Managers")]
        [SerializeField] 
        private FoodManager _FoodManager;
        [FormerlySerializedAs("_DeersManager")]
        [SerializeField]
        private DeerManager _DeerManager;
        [FormerlySerializedAs("_WolfsManager")]
        [SerializeField]
        private WolvesManager _WolvesManager;

        public static EnvironmentController Instance;
        public Bounds ArenaBounds => _ArenaBounds;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Academy.Instance.OnEnvironmentReset += ResetEnvironment;
            }
            else
            {
                Destroy(this);
            }
        }
        
        private void OnDrawGizmos()
        {
            var position = transform.position;
            _ArenaBounds?.DrawBounds(position);
            if (_FoodManager)
                _FoodManager.DrawBounds(position);
            if (_DeerManager)
                _DeerManager.DrawBounds(position);
            if (_WolvesManager)
                _WolvesManager.DrawBounds(position);
        }

        private void ResetEnvironment()
        {
            _FoodManager.ResetFood();
            _DeerManager.ResetAgents();
            _WolvesManager.ResetAgents();
        }
    }
}
