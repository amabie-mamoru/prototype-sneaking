using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain
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
        private List<GameObject> foundObjects;
        public List<IBody> FoundObjects {
            get {
                List<IBody> bodies = new List<IBody>();
                foreach (var obj in foundObjects)
                {
                    var body = obj.GetComponent<IBody>();
                    if (body == null) {
                        throw new SightException($"{obj.name} attempts no Body script.");
                    }
                    bodies.Add(body);
                }
                return bodies;
            }
        }

        /// <summary>
        /// 気配を捉えているかどうか
        /// </summary>
        public bool FeelSigns => ObjectsInSight.Count > 0;

        /// <summary>
        /// 視界に捉えているかどうか
        /// </summary>
        public bool IsFound => foundObjects.Count > 0;

        private uint foundCounter = 0;
        private uint lostCounter = 0;

        private List<Vector3> eyePositions;

        public Sight()
        {
            ObjectsInSight = new List<GameObject>();
            foundObjects = new List<GameObject>();
        }

        public void SetEyes(List<Vector3> eyePositions)
        {
            this.eyePositions = eyePositions;
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
                        + $"{foundObjects.Count} gameObjects unexcepted. "
                        + $"(e.g. {FoundObjects[0].Name} is found, but no feel sign)";
                throw new SightException(msg);
            }
            if (ObjectsInSight.Count != foundObjects.Count)
            {
                throw new SightException("今のところ数ずれるのはナシ！");
            }
        }
#endif

        public void OnTriggerEnter(Collider other)
        {
            Include(other.gameObject);
        }

        public void OnTriggerStay(Collider _other)
        {
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
            int layerMask = ~(1 << 2 | 1 << 11);
            if (!Physics.Raycast(eyePosition, direction, out RaycastHit hitinfo, maxDistance, ~layerMask)) {
                // NOTE: ここには絶対こない想定
                // TriggerStay している時点で目的の GameObject or それ以外の障害物にぶつかるはずなので。
                throw new SightException($"Except {gameObj.name}, but cannot find. The position is ({gameObjTargetPosition}).");
            }
            var firstHitGameObject = hitinfo.collider.gameObject;
            if (WantToFind(firstHitGameObject))
            {
                if (foundObjects.Exists(obj => obj.GetInstanceID() == gameObj.GetInstanceID())) { return; }
                foundCounter++;
                foundObjects.Add(firstHitGameObject);
            }
            else {
                var index = foundObjects.FindIndex(obj => obj.GetInstanceID() == gameObj.GetInstanceID());
                if (index == -1) { return; }
                lostCounter++;
                foundObjects.RemoveAt(index);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            Exclude(other.gameObject);
        }

        private void Include(GameObject gameObj)
        {
            if (ObjectsInSight.Exists(obj => obj.GetInstanceID() == gameObj.GetInstanceID())){ return; }
            if (!WantToFind(gameObj)) { return; }
            ObjectsInSight.Add(gameObj);
        }

        private void Exclude(GameObject gameObj)
        {
            var index = ObjectsInSight.FindIndex(obj => obj.GetInstanceID() == gameObj.GetInstanceID());
            if (index == -1) { return; }
            ObjectsInSight.RemoveAt(index);

            // 障害物なしに視界の外に出てしまった場合
            index = foundObjects.FindIndex(obj => obj.GetInstanceID() == gameObj.GetInstanceID());
            if (index == -1) { return; }
            lostCounter++;
            foundObjects.RemoveAt(index);
        }

        private bool WantToFind(GameObject gameObj)
        {
            return gameObj.GetComponent<IBody>() != null;
        }
    }

    public class SightException : System.Exception {
        public SightException(string message) : base(message) { }
    }
}
