using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain.Stage
{
    public class Sight : MonoBehaviour
    {
        /// <summary>
        /// 視野となりうる領域に含まれている物質（障害物は考慮しない）
        /// </summary>
        private List<GameObject> ObjectsInSight;

        /// <summary>
        /// 視界に捉えている物質
        /// </summary>
        [System.NonSerialized]
        public List<GameObject> FoundObjects;

        /// <summary>
        /// 気配を捉えているかどうか
        /// </summary>
        public bool FeelSigns => ObjectsInSight.Count > 0;

        /// <summary>
        /// 視界に捉えているかどうか
        /// </summary>
        public bool IsFound => FoundObjects.Count > 0;

        private uint foundCounter = 0;
        private uint lostCounter = 0;

        private List<Vector3> eyePositions;

        public GameObject GameObject => gameObject;

        private bool Enabled;

        public Sight()
        {
            ObjectsInSight = new List<GameObject>();
            FoundObjects = new List<GameObject>();
            Enabled = true;
        }

        public void SetEyes(List<Vector3> eyePositions)
        {
            this.eyePositions = eyePositions;
        }

        public void ToDisable()
        {
            ObjectsInSight = new List<GameObject>();
            FoundObjects = new List<GameObject>();
            foundCounter = 0;
            lostCounter = 0;
            Enabled = false;
        }

        public void ToEnable()
        {
            Enabled = true;
        }

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
            ObjectsInSight = new List<GameObject>();
        }

#if UNITY_EDITOR
        // TODO: デバッグ用の例外処理（リリース時には取り除く）
        Character character;
        public void SetCharacter(Character character)
        {
            this.character = character;
        }

        // ライフサイクル的に Update だと、1フレームに1回以上よばれる（physics cycle may happen more than once per frame）
        // と公式文章にもあるので Trigger が1回よばれる毎に必ず1回は確認する FixedUpdate の段階でチェックする
        // Debug.Log を出力したところ more than とかいうけど less than なケースもありそう
        public void FixedUpdate()
        {
            // 何かを見つけてる状態なのに気配はない状態はありえない
            if (IsFound && !FeelSigns)
            {
                var msg = $"GameObject ({character.Name}) has unexcepted collision. "
                        + $"{FoundObjects.Count} gameObjects unexcepted. "
                        + $"(e.g. {FoundObjects[0].name} is found, but no feel sign)";
                throw new SightException(msg);
            }
            if (ObjectsInSight.Count < FoundObjects.Count)
            {
                throw new SightException("FoundObjects are more than ObjectsInSight");
            }
        }
#endif

        public void OnTriggerEnter(Collider other)
        {
            if (!Enabled) { return; }
            Include(other.gameObject);
        }

        public void OnTriggerStay(Collider _other)
        {
            if (!Enabled) { return; }
            if (ObjectsInSight.Count == 0) { return; }
            foreach (var eyePosition in eyePositions)
            {
                foreach (var gameObj in ObjectsInSight)
                {
                    FindOrLoseWithRay(eyePosition, gameObj);
                }
            }
        }

        private void FindOrLoseWithRay(Vector3 eyePosition, GameObject gameObj)
        {
            var edges = gameObj.GetComponent<Character>()?.Edges;
            // edges を設定していないオブジェクトの判定
            if (edges == null)
            {
                FindOrLoseWithRay(eyePosition, gameObj.transform.position, gameObj);
                return;
            }
            foreach (var edgePosition in edges)
            {
                FindOrLoseWithRay(eyePosition, edgePosition, gameObj);
            }            
        }

        private void FindOrLoseWithRay(Vector3 eyePosition, Vector3 gameObjTargetPosition, GameObject gameObj)
        {
            var direction = Vector3.Normalize(gameObjTargetPosition - eyePosition);
            var maxDistance = Vector3.Magnitude(gameObjTargetPosition - eyePosition) + 1f; // ちょっと長めに設定しておく
            int layerMask = ~(1 << 2);
            if (!Physics.Raycast(eyePosition, direction, out RaycastHit hitinfo, maxDistance, layerMask))
            {
                // 間にオブジェクトの一部自体は入っているが、edge が設置されているとき含まれないケースのときにここにくる
                return;
            }

            var firstHitGameObject = hitinfo.collider.gameObject;
            if (WantToFind(firstHitGameObject))
            {
                if (FoundObjects.Exists(obj => obj.GetInstanceID() == firstHitGameObject.GetInstanceID())) { return; }
                foundCounter++;
                FoundObjects.Add(firstHitGameObject);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (!Enabled) { return; }
            Exclude(other.gameObject);
        }

        private void Include(GameObject gameObj)
        {
            if (!WantToFind(gameObj)) { return; }
            if (ObjectsInSight.Exists(obj => obj.GetInstanceID() == gameObj.GetInstanceID())){ return; }
            ObjectsInSight.Add(gameObj);
        }

        private void Exclude(GameObject gameObj)
        {
            if (!WantToFind(gameObj)) { return; }
            var index = ObjectsInSight.FindIndex(obj => obj.GetInstanceID() == gameObj.GetInstanceID());
            if (index == -1) { return; }
            ObjectsInSight.RemoveAt(index);

            // 障害物なしに視界の外に出てしまった場合
            index = FoundObjects.FindIndex(obj => obj.GetInstanceID() == gameObj.GetInstanceID());
            if (index == -1) { return; }
            lostCounter++;
            FoundObjects.RemoveAt(index);
        }

        private bool WantToFind(GameObject gameObj)
        {
            // タグも用意しているがあえて継承状況を確認している
            // タグを使うメリット : GetComponent より高速
            // 継承を使うメリット : スクリプトを付与することを強制するので設定漏れを回避しやすい
            return gameObj.GetComponent<Detectable>() != null;
        }
    }

    public class SightException : System.Exception {
        public SightException(string message) : base(message) { }
    }
}
