using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PrototypeSneaking.Domain.Stage
{
    public class LinearTrackingHabit : CharacterHabit
    {
        private List<Vector3> accessPoints;
        private Vector3 originalLookAtPosition;
        private Quaternion capturedBeginningSearchedQuaternion;
        private Vector3 capturedBeginningSearchedPosition;
        private Quaternion capturedLastSearchedQuaternion;
        private Vector3 capturedLastSearchedPosition;
        private float searchingElapsedRatio;
        private ILinearTrackingHabitStateMachine state;
        private GameObject trackingObj;

        private const float ALLOWABLE_ERROR = 0.08f; // NOTE: 現状だと 0.0666... に収束してしまったので少し大きめの値に設定
        [SerializeField] private float rotationSpeed = 2f;

        private void Awake()
        {
            state = new LinearTrackingHabitStateMachine();
            accessPoints = new List<Vector3>();
        }

        public override void Behave()
        {
            // State 更新 => Position 更新 の順
            UpdateState();
            UpdatePosition();
        }

        /// <summary>
        /// 内部メソッドは State の更新が主な責務
        /// </summary>
        private void UpdateState()
        {
#if UNITY_EDITOR
            state.DebugLog(character.Name); // TODO: Debug
#endif
            CheckToAccessPoint();
            CheckToLoseAttackObj();
            CheckToSearchObj();
            CheckToAttackDistance();
            CheckSight();
        }

        /// <summary>
        /// 視界のチェック
        /// </summary>
        private void CheckSight()
        {
            if (!state.IsWondering) { return; }
            // 見失ってたら
            if (!sight.IsFound) { return; }
            // 何かを見つけたら
            if (sight.GetFoundCountAndReset() != 0)
            {
                var closest = Mathf.Infinity;
                GameObject closestObj = new GameObject();
                sight.FoundObjects.ForEach(obj => {
                    var distance = Vector3.Magnitude(character.transform.position - sight.FoundObjects[0].transform.position);
                    if (distance < closest) { closestObj = obj; }
                });
                trackingObj = closestObj;
                BeginToTrack(closestObj);
            }
        }


        /// <summary>
        /// 何かを見つけた時に最初に一度だけ呼ばれる
        /// </summary>
        private void BeginToTrack(GameObject foundObj)
        {
            state.ToTrack();
            agent.SetDestination(foundObj.transform.position);
            accessPoints.Add(character.transform.position);
            accessPoints.Add(foundObj.transform.position);
            originalLookAtPosition = foundObj.transform.position;
        }

        /// <summary>
        /// 何かを見失った時に一度だけ呼ばれる
        /// </summary>
        private void CheckToLoseAttackObj()
        {
            if (!state.IsLostAttackObj) { return; }
            state.ToGoBack();
            accessPoints.RemoveAt(accessPoints.Count - 1);
            agent.SetDestination(accessPoints.Last());
            character.transform.LookAt(accessPoints.Last());
        }

        /// <summary>
        /// 目的の場所に到達したかのチェック
        /// </summary>
        private void CheckToAttackDistance()
        {
            if (!state.IsTracking) { return; }
            var isClose = Vector3.Magnitude(character.transform.position - accessPoints.Last()) <= character.RadiusWithMargin;
            if (state.IsTracking)
            {
                if (isClose) { ReachTargetPosition(); }
            }
        }

        private void CheckToAccessPoint()
        {
            if (!state.IsGoBack) { return; }
            Debug.Log(Vector3.Magnitude(character.transform.position - accessPoints.Last()));
            var isJust = Vector3.Magnitude(character.transform.position - accessPoints.Last()) <= ALLOWABLE_ERROR;
            if (!isJust) { return; }
            if (accessPoints.Count == 1)
            {
                WentBackToOriginalPosition();
            }
            else
            {
                WentBackToAccessPoint();
            }
        }

        private void CheckToSearchObj()
        {
            if (!state.IsSearchingAttackObj) { return; }
            // 見失ってたら
            if (searchingElapsedRatio == 1f)
            {
                state.ToLoseAttackObj();
                UnityEngine.Debug.Log("Lost!!!");
                return;
            }

            if (!sight.IsFound) { return; }
            // 何かを見つけたら
            if (sight.GetFoundCountAndReset() != 0)
            {
                var closest = Mathf.Infinity;
                GameObject closestObj = new GameObject();
                sight.FoundObjects.ForEach(obj => {
                    var distance = Vector3.Magnitude(character.transform.position - sight.FoundObjects[0].transform.position);
                    if (distance < closest) { closestObj = obj; }
                });
                state.ToDisable();
                UnityEngine.Debug.Log("Kill!!!");
            }
        }

        private void ReachTargetPosition()
        {
            // NOTE: 障害物にぶつからない isClose な場所で Reach だと判断したとき、 agent の Destination とズレるので合わせる
            agent.SetDestination(character.transform.position);

            state.ToReachAttackDistance();
            state.ToSearchAttackObj();
            capturedBeginningSearchedPosition = character.transform.position;
            capturedBeginningSearchedQuaternion = character.transform.rotation;
            capturedLastSearchedPosition = trackingObj.transform.position;

            // SearchAttackObj で参照する capturedLastSearchedQuaternion
            // 都度計算の必要がないため、到達した段階で一度だけ実行する
            Vector3 direction = capturedLastSearchedPosition - capturedBeginningSearchedPosition;
            capturedLastSearchedQuaternion = Quaternion.LookRotation(direction, Vector3.up);
            searchingElapsedRatio = 0f;
        }

        /// <summary>
        /// 元の場所に戻ってきた時の State の更新
        /// </summary>
        private void WentBackToOriginalPosition()
        {
            accessPoints.RemoveAt(0);
            state.ToWonder();
            // originalLookAtPosition はとりあえず初期化しない
            character.transform.LookAt(originalLookAtPosition);
        }

        /// <summary>
        /// アクセスポイントまで戻ってきた場合
        /// </summary>
        private void WentBackToAccessPoint()
        {
            accessPoints.RemoveAt(accessPoints.Count - 1);
            agent.SetDestination(accessPoints.Last());
        }

        /// <summary>
        /// 内部メソッドは Position の更新が主な責務
        /// </summary>
        private void UpdatePosition()
        {
            DecideAction();
        }

        private void DecideAction()
        {
            GoBack();
            SearchAttackObj();
            Tracking();
            Wondering();
        }

        private void Wondering()
        {
            if (!state.IsWondering) { return; }
        }

        private void Tracking()
        {
            if (!state.IsTracking) { return; }
            // 記事による回転変更
            // 「【Unity】NavMeshAgentの経路だけを参照して移動処理に利用したお話」
            // 「NavMeshAgentの挙動を手動でアップデートする」

            // 位置の更新
            character.transform.position = agent.nextPosition;
        }

        private void SearchAttackObj()
        {
            if (!state.IsSearchingAttackObj) { return; }
            //Vector3 direction = capturedLastSearchedPosition - capturedBeginningSearchedPosition;
            //var toRotation = Quaternion.LookRotation(direction, Vector3.up); => capturedLastSearchedQuaternion
            searchingElapsedRatio += rotationSpeed * Time.deltaTime;
            if (searchingElapsedRatio > 1f) {
                searchingElapsedRatio = 1;
            }
            // TODO: これが本当に 100% になるかは未確認。ならない場合、無限にここの処理を通る
            character.transform.rotation = Quaternion.Lerp(capturedBeginningSearchedQuaternion, capturedLastSearchedQuaternion, searchingElapsedRatio);
        }

        private void GoBack()
        {
            if (!state.IsGoBack) { return; }
            // 記事による回転変更
            // 「【Unity】NavMeshAgentの経路だけを参照して移動処理に利用したお話」
            // 「NavMeshAgentの挙動を手動でアップデートする」

            // 位置の更新
            character.transform.position = agent.nextPosition;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (agent && agent.enabled)
            {
                Gizmos.color = Color.red;
                var prefPos = transform.position;

                foreach (var pos in agent.path.corners)
                {
                    Gizmos.DrawLine(prefPos, pos);
                    prefPos = pos;
                }
            }
        }
#endif
    }

    public class LinearTrackingHabitException : System.Exception {
        public LinearTrackingHabitException(string message) : base(message) { }
    }
}