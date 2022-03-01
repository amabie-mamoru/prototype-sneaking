using UnityEngine;
using UnityEngine.AI;

namespace PrototypeSneaking.Domain.Stage
{
    public class Automonus : Character
    {
        [SerializeField] private NavMeshAgent agent;
        private Vector3 beginToTrackPosition;
        private GameObject trackingObject;
        private WalkStatus walkStatus;
        enum WalkStatus
        {
            None,
            Tracking,
            GoBack
        }

        protected override void Awake()
        {
            base.Awake();

            // NOTE: 経路構築は NavMeshAgent に任せたいが移動や回転はこちらでやりたいので。
            agent.updatePosition = false;
            agent.updateRotation = false;
            walkStatus = WalkStatus.None;
            agent.stoppingDistance = 0;
        }
        
        public void BeginToTrack(GameObject foundObject)
        {
            if (walkStatus == WalkStatus.Tracking) { return; }
            beginToTrackPosition = gameObject.transform.position;
            trackingObject = foundObject;
            agent.SetDestination(foundObject.transform.position);
            walkStatus = WalkStatus.Tracking;
        }

        // TODO: NavMeshAgent の Stopping distance で発火できる何かがあればそちらでもよさそう
        public void LoseObject()
        {
            if (walkStatus == WalkStatus.GoBack) { return; }
            trackingObject = null;
            agent.SetDestination(beginToTrackPosition);
            walkStatus = WalkStatus.GoBack;
            transform.forward = -transform.forward;
        }

        public void ReachOrigin()
        {
            if (walkStatus != WalkStatus.GoBack) { return; }
            agent.ResetPath();
            transform.forward = -transform.forward; // 元に戻す
            walkStatus = WalkStatus.None;
        }

        protected override void Update()
        {
            base.Update();
            UpdateByNavMeshAgent();
        }

        protected override void CheckSight()
        {
            if (sight.GetLostCountAndReset() != 0) { LoseObject(); }
            if (!sight.IsFound) { return; }
            // 最初のオブジェクトを検出
            if (sight.GetFoundCountAndReset() != 0) { BeginToTrack(sight.FoundObjects[0]); }
            //sight.FoundObjects.ForEach(obj => Debug.Log($"[Character] {gameObject.name} found {obj.name}({obj.GetInstanceID()}) ({Time.time} [sec])"));
        }

        private void UpdateByNavMeshAgent()
        {
            if (walkStatus == WalkStatus.None) { return; }
            UpdateToTrack();
            UpdateToLose();
        }

        private void UpdateToTrack()
        {
            var dir = agent.nextPosition - transform.position;
            // 位置の更新
            transform.position = agent.nextPosition;
            if (!sight.IsFound) { return; }
            // TODO: あとでオブジェクトの大きさを取ってくるようにする
            if (Vector3.Magnitude(transform.position - sight.FoundObjects[0].transform.position) <= 2)
            {
                LoseObject();
                sight.ToDisable();
            }
        }

        private void UpdateToLose()
        {
            if (walkStatus != WalkStatus.GoBack) { return; }
            if (Vector3.Magnitude(transform.position - agent.destination) <= 0.001f)
            {
                ReachOrigin();
                sight.ToEnable();
            }
        }

#if UNITY_EDITOR
        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 2) { return; }
            if (trackingObject == null) { return; }
            if (trackingObject.GetInstanceID() != collision.gameObject.GetInstanceID()) { return; }
            // TODO: 本来はここで終わりだが。一旦ログ吐いてやり直す。このとき、一旦 Sight は無効にしておく
            // 実際に Catch する前に終わりたいのでここにはこないようにする
            Debug.LogError($"Catch {collision.gameObject.name}!!");
        }
#endif
    }
}