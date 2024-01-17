// Author: Jan Vaculik

using System;
using UnityEngine;

namespace Environment
{
    [Serializable]
    public abstract class ASpawner<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField]
        protected Bounds _SpawnArea;
        
        [SerializeField]
        protected T _Prefab;
        
        public void DrawBounds(Vector3 position) => _SpawnArea.DrawBounds(position);
            
        public virtual T Spawn()
        {
            var instantiatedObj = Instantiate(_Prefab, _SpawnArea.GetRandomPosition(), Quaternion.identity, transform);
            return instantiatedObj;
        }
    }
}
