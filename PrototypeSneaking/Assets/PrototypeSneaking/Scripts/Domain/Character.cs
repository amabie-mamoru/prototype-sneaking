using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain
{
    public interface ICharacter
    {
        GameObject GameObject { get; }
    }

    public class Character : MonoBehaviour, ICharacter
    {
        [SerializeField] private Sight sight;
        [SerializeField] private Sensor sensor;
        public GameObject GameObject => gameObject;
        public List<Vector3> Edges => sensor.Edges;

        public void Awake()
        {
            sight.SetEyes(sensor.Eyes);
#if UNITY_EDITOR
            // TODO: ???????????????????????
            sight.SetCharacter(this);
#endif
        }

        public void Update()
        {
            CheckSight();
        }

        private void CheckSight()
        {
            if (sight.GetLostCountAndReset() != 0) { Debug.Log("[Charcter] lost!"); }
            if (!sight.IsFound) { return; }
            if (sight.GetFoundCountAndReset() != 0) { Debug.Log("[Character] found!"); }
            sight.FoundObjects.ForEach(obj => Debug.Log($"[Character] {gameObject.name} found {obj.gameObject.name}({obj.GetInstanceID()}) ({Time.time} [sec])"));
        }
    }
}