using UnityEngine;

namespace PrototypeSneaking.Domain
{
    public interface ICharacter
    {
    }

    public class Character : MonoBehaviour, ICharacter
    {
        [SerializeField] private Sight sight;

        public void Update()
        {
            CheckSight();
        }

        private void CheckSight()
        {
            if (sight.GetLostCountAndReset() != 0) { Debug.Log("lost!"); }
            if (!sight.IsFindingSomething) { return; }
            if (sight.GetFoundCountAndReset() != 0) { Debug.Log("found!"); }
            sight.CapturedObjects.ForEach(obj => Debug.Log($"[Character] {gameObject.name} found {obj.gameObject.name}"));
        }
    }
}