using UnityEngine;
using UnityEngine.AI;

namespace PrototypeSneaking.Domain.Stage
{
    public class Automonus : Character
    {
        [SerializeField] protected Sight sight;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private CharacterHabit habit;

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

            habit.TellParent(this, sight, agent);
        }

        protected void Update()
        {
            habit.Behave();
        }
    }
}