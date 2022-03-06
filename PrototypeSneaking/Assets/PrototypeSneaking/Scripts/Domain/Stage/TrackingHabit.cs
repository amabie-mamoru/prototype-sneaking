using UnityEngine;

namespace PrototypeSneaking.Domain.Stage
{
    public class TrackingHabit : CharacterHabit
    {
        private Vector3 beginToTrackPosition;
        private GameObject trackingObject;
        private WalkStatus walkStatus;
        enum WalkStatus
        {
            None,
            Tracking,
            GoBack
        }

        private void Awake()
        {
            walkStatus = WalkStatus.None;
        }

        public override void Behave()
        {
            CheckSight();
            UpdateByNavMeshAgent();
        }

        private void BeginToTrack(GameObject foundObject)
        {
            if (walkStatus == WalkStatus.Tracking) { return; }
            beginToTrackPosition = character.transform.position;
            trackingObject = foundObject;
            agent.SetDestination(foundObject.transform.position);
            walkStatus = WalkStatus.Tracking;
        }

        // TODO: NavMeshAgent の Stopping distance で発火できる何かがあればそちらでもよさそう
        private void LoseObject()
        {
            if (walkStatus == WalkStatus.GoBack) { return; }
            trackingObject = null;
            agent.SetDestination(beginToTrackPosition);
            walkStatus = WalkStatus.GoBack;
            character.transform.forward = -character.transform.forward;
        }

        private void ReachOrigin()
        {
            if (walkStatus != WalkStatus.GoBack) { return; }
            agent.ResetPath();
            character.transform.forward = -character.transform.forward; // 元に戻す
            walkStatus = WalkStatus.None;
        }

        private void CheckSight()
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
            var dir = agent.nextPosition - character.transform.position;
            // 位置の更新
            character.transform.position = agent.nextPosition;
            if (!sight.IsFound) { return; }
            // TODO: あとでオブジェクトの大きさを取ってくるようにする
            if (Vector3.Magnitude(character.transform.position - sight.FoundObjects[0].transform.position) <= 2)
            {
                LoseObject();
                sight.ToDisable();
            }
        }

        private void UpdateToLose()
        {
            if (walkStatus != WalkStatus.GoBack) { return; }
            if (Vector3.Magnitude(character.transform.position - agent.destination) <= 0.001f)
            {
                ReachOrigin();
                sight.ToEnable();
            }
        }
    }
}