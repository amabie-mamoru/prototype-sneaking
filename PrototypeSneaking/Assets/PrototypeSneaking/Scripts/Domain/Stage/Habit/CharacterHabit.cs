using System;
using UnityEngine;
using UnityEngine.AI;

namespace PrototypeSneaking.Domain.Stage
{
    public interface ICharacterHabit
    {
        void TellParent(Automonus character, Sight sight, NavMeshAgent agent);
        void Behave();
    }

    public class CharacterHabit : MonoBehaviour, ICharacterHabit
    {
        protected Automonus character;
        protected NavMeshAgent agent;
        protected Sight sight;

        public virtual void TellParent(Automonus character, Sight sight, NavMeshAgent agent)
        {
            this.character = character;
            this.agent = agent;
            this.sight = sight;
            DisableAgent();
        }

        protected void DisableAgent()
        {
            // NOTE: 経路構築は NavMeshAgent に任せたいが移動や回転はこちらでやりたいので無効化する
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.autoBraking = true;
            agent.stoppingDistance = 0;
        }

        public virtual void Behave()
        {
            throw new NotImplementedException();
        }
    }
}