using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain
{
    public interface ICharacter
    {
        GameObject GameObject { get; }
        void DisableSight();
    }

    public class Character : MonoBehaviour, ICharacter
    {
        [SerializeField] private Sight sight;
        [SerializeField] private Sensor sensor;
        public List<Vector3> Edges => sensor.Edges;
        public string Name => gameObject.name;
        public GameObject GameObject => gameObject;

        public void Awake()
        {
            sight.SetEyes(sensor.Eyes);
#if UNITY_EDITOR
            // TODO: デバッグ用の例外処理
            sight.SetCharacter(this);
#endif
        }
        public void DisableSight()
        {
            sight.GameObject.SetActive(false);
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
            sight.FoundObjects.ForEach(obj => Debug.Log($"[Character] {gameObject.name} found {obj.name}({obj.GetInstanceID()}) ({Time.time} [sec])"));
        }
    }
}