using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain.Stage
{
    public interface ICharacter
    {
        GameObject GameObject { get; }
    }

    public class Character : MonoBehaviour, ICharacter
    {
        [SerializeField] protected Sensor sensor;
        public List<Vector3> Edges => sensor.Edges;
        public string Name => gameObject.name;
        public GameObject GameObject => gameObject;
    }
}