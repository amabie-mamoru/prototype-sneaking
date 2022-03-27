using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain.Stage
{
    public interface ICharacter
    {
        GameObject GameObject { get; }
    }

    public class Character : Detectable, ICharacter
    {
        [SerializeField] protected Sensor sensor;
        public List<Vector3> Eyes => sensor.Eyes;
        public List<Vector3> Edges => sensor.Edges;
        public string Name => gameObject.name;
        public GameObject GameObject => gameObject;

        protected float radius;
        protected const float MARGIN_BETWEEN_OBSTACLE = 0.2f; // NOTE: 暫定
        /// <summary>
        /// ぶつからない程度の余裕を持った半径 ()
        /// </summary>
        public float RadiusWithMargin {
            get {
                if (radius == 0f)
                {
                    radius = GetRadiusByBoxCollider();
                }
                return radius + MARGIN_BETWEEN_OBSTACLE;
            }
        }

        /// <summary>
        /// Box Collider の場合、対角距離
        /// </summary>
        private float GetRadiusByBoxCollider()
        {
            var collider = GetComponent<BoxCollider>();
            if (collider == null) { return 0; }
            return Mathf.Sqrt(Mathf.Pow(collider.size.x / 2f, 2) + Mathf.Pow(collider.size.z / 2f, 2));
        }
    }
}