using System.Collections.Generic;
using UnityEngine;


namespace PrototypeSneaking.Domain
{
    public class Sight : MonoBehaviour
    {
        /// <summary>
        /// 視野に捉えている GameObject
        /// </summary>
        public List<GameObject> CapturedObjects;

        /// <summary>
        /// 視野に何かを捉えている
        /// </summary>
        public bool IsFindingSomething => CapturedObjects.Count > 0;
        private uint foundCounter = 0;
        private uint lostCounter = 0;

        /// <summary>
        /// 何かに気づいたアニメーションをするときに使う
        /// </summary>
        public uint GetFoundCountAndReset()
        {
            var cnt = foundCounter;
            foundCounter = 0;
            return cnt;
        }

        /// <summary>
        /// 何かを見失ったアニメーションをするときに使う
        /// </summary>
        public uint GetLostCountAndReset()
        {
            var cnt = lostCounter;
            lostCounter = 0;
            return cnt;
        }

        public void Awake()
        {
            CapturedObjects = new List<GameObject>();
        }

        public void OnTriggerEnter(Collider other)
        {
            foundCounter++;
            Capture(other.gameObject);
        }

        public void OnTriggerExit(Collider other)
        {
            lostCounter++;
            Lose(other.gameObject);
        }

        private void Capture(GameObject gameObj)
        {
            if (CapturedObjects.Exists(obj => obj.GetInstanceID() == gameObj.GetInstanceID())) { return; }
            CapturedObjects.Add(gameObj);
        }

        private void Lose(GameObject gameObj)
        {
            var index = CapturedObjects.FindIndex(obj => obj.GetInstanceID() == gameObj.GetInstanceID());
            if (index == -1) { return; }
            CapturedObjects.RemoveAt(index);
        }
    }
}
