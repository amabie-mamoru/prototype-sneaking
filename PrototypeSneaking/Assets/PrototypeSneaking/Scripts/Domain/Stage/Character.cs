using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain.Stage
{
    public interface ICharacter
    {
        GameObject GameObject { get; }
        void ToControll();
    }

    public class Character : MonoBehaviour, ICharacter
    {
        [SerializeField] protected Sight sight;
        [SerializeField] private Sensor sensor;
        public List<Vector3> Edges => sensor.Edges;
        public string Name => gameObject.name;
        public GameObject GameObject => gameObject;

        protected virtual void Awake()
        {
            sight.SetEyes(sensor.Eyes);
#if UNITY_EDITOR
            // TODO: デバッグ用の例外処理
            sight.SetCharacter(this);
#endif
            // エディタいじってたら off にすることもあると思うので。
            if (!sight.GameObject.activeSelf)
            {
                sight.GameObject.SetActive(true);
            }
        }

        public void ToControll()
        {
            sight.GameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            CheckSight();
        }

        protected virtual void CheckSight()
        {
#if UNITY_EDITOR
            // 見失った際の挙動
            if (sight.GetLostCountAndReset() != 0) { Debug.Log("[Charcter] lost!"); }
            // 何か見つけた場合は以下を処理する
            if (!sight.IsFound) { return; }
            // 見つけた際の挙動
            if (sight.GetFoundCountAndReset() != 0) { Debug.Log("[Character] found!"); }
            // 見つけたオブジェクトに対しての作業
            sight.FoundObjects.ForEach(obj => Debug.Log($"[Character] {gameObject.name} found {obj.name}({obj.GetInstanceID()}) ({Time.time} [sec])"));
#endif
        }
    }
}