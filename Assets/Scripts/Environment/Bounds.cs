// Author: Jan Vaculik

using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Environment
{
    [Serializable]
    public class Bounds
    {
        [SerializeField]
        private Vector3 _Min;
        [SerializeField]
        private Vector3 _Max;
        [SerializeField]
        private Color _Color;

        public Vector3 Min => _Min;
        public Vector3 Max => _Max;
        public Color Color => _Color;
        
        public Vector3 GetRandomPosition() {
            
            return new Vector3(
                Random.Range(Min.x, Max.x),
                Random.Range(Min.y, Max.y),
                Random.Range(Min.z, Max.z)
            );
        }
        
        public void DrawBounds(Vector3 position)
        {
            Gizmos.color = Color;

            var corner1 = new Vector3(Min.x, 0, Min.z) + position;
            var corner2 = new Vector3(Max.x, 0, Min.z) + position;
            var corner3 = new Vector3(Max.x, 0, Max.z) + position;
            var corner4 = new Vector3(Min.x, 0, Max.z) + position;

            Gizmos.DrawLine(corner1, corner2);
            Gizmos.DrawLine(corner2, corner3);
            Gizmos.DrawLine(corner3, corner4);
            Gizmos.DrawLine(corner4, corner1);
        }
        
        public bool Contains(Vector3 position)
        {
            return position.x >= Min.x && position.x <= Max.x &&
                   position.y >= Min.y && position.y <= Max.y &&
                   position.z >= Min.z && position.z <= Max.z;
        }
        
        public Vector3 GetCenter()
        {
            return new Vector3(
                (Min.x + Max.x) / 2,
                (Min.y + Max.y) / 2,
                (Min.z + Max.z) / 2
            );
        }

        public Vector3 ClosestPoint(Vector3 position) { 
            var x = Mathf.Clamp(position.x, Min.x, Max.x);
            var y = Mathf.Clamp(position.y, Min.y, Max.y);
            var z = Mathf.Clamp(position.z, Min.z, Max.z);

            return new Vector3(x, y, z);
        }
    }
}
