using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain
{
    public interface ICharacter
    {
        void Translate(Vector3 deltaMovement);
        void Translate(float deltaX, float deltaY, float deltaZ);
    }

    public class Character : MonoBehaviour, ICharacter
    {
        [SerializeField] private Sight sight;
        [SerializeField] private Sensor sensor;
        [SerializeField] private Body body;
        public List<Vector3> Edges => sensor.Edges.GetPositions();
        public string Name => gameObject.name;

        public void Awake()
        {
            sight.SetEyes(sensor.Eyes);
            body.Tell(Name);
            sensor.Edges.Translate(gameObject.transform.position);
#if UNITY_EDITOR
            // TODO: デバッグ用の例外処理
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
            sight.FoundObjects.ForEach(obj => Debug.Log($"[Character] {gameObject.name} found {obj.Name}({obj.GetInstanceID()}) ({Time.time} [sec])"));
        }

        public void Translate(Vector3 deltaMovement)
        {
            sensor.Edges.Translate(deltaMovement);
            gameObject.transform.Translate(deltaMovement);
        }

        public void Translate(float deltaX, float deltaY, float deltaZ)
        {
            Translate(new Vector3(deltaX, deltaY, deltaZ));
        }
    }
}