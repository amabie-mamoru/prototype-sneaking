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
            sight.SetCharacter(this);

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