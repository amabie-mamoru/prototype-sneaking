using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PrototypeSneaking.Domain.Stage
{
    public class StraightTrackingHabit : CharacterHabit
    {
        private List<Vector3> accessPoints;
        private Quaternion originalRotation;
        private Quaternion currentRotation;
        private IStraightTrackingHabitStateMachine state;

        private const float ALLOWABLE_ERROR = 0.0001f;

        private void Awake()
        {
            state = new StraightTrackingHabitStateMachine();
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
            CheckSight();
            CheckToTargetPosition();
        }

        /// <summary>
        /// 視界のチェック
        /// </summary>
        private void CheckSight()
        {
            if (state.IsTracking || state.IsGoingBack || state.IsJustLost) { return; }
                if (sight.FoundObjects.Count != 0) {
                    sight.FoundObjects.ForEach(obj => {
                        Debug.Log($"found: {obj.name}");
                    });
                }
            // 何かを見失ったら
            if (sight.GetLostCountAndReset() != 0) { LostObject(); }
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
                BeginToTrack(closestObj);
            }
        }


        /// <summary>
        /// 何かを見つけた時に最初に一度だけ呼ばれる
        /// </summary>
        private void BeginToTrack(GameObject foundObj)
        {
            state.ToTrack();
            accessPoints.Add(foundObj.transform.position);
            originalRotation = foundObj.transform.rotation;
            currentRotation = Quaternion.LookRotation(foundObj.transform.position);
        }

        /// <summary>
        /// 何かを見失った時に一度だけ呼ばれる
        /// </summary>
        private void LostObject()
        {
            state.JustToLose();
        }

        /// <summary>
        /// 目的の場所に到達したかのチェック
        /// </summary>
        private void CheckToTargetPosition()
        {
            if (!(state.IsTracking || state.IsGoingBack)) { return; }
            var isClose = Vector3.Magnitude(character.transform.position - accessPoints.Last()) <= character.RadiusWithMargin;
            if (state.IsTracking)
            {
                if (isClose) { ReachTargetPosition(); }
            }
            else if (state.IsGoingBack)
            {
                if (isClose)
                {
                    if (accessPoints.Count == 1)
                    {
                        var isJust = Vector3.Magnitude(character.transform.position - accessPoints.Last()) <= ALLOWABLE_ERROR;
                        if (isJust) { WentBackToOriginalPosition(); }
                    }
                    else
                    {
                        WentBackToAccessPoint();
                    }
                }
            }
            else
            {
                throw new StraightTrackingHabitException("非同期で一つの StraightTrackingHabit を操作している可能性があります");            
            }
            
        }

        private void ReachTargetPosition()
        {
            accessPoints.Add(character.transform.position);
            state.ToReachAttackDistance();
        }

        /// <summary>
        /// 元の場所に戻ってきた時の State の更新
        /// </summary>
        private void WentBackToOriginalPosition()
        {
            accessPoints.RemoveAt(0);
            state.ToWonder();
            character.transform.rotation = originalRotation;
            originalRotation = Quaternion.identity;
        }

        /// <summary>
        /// アクセスポイントまで戻ってきた場合
        /// </summary>
        private void WentBackToAccessPoint()
        {
            accessPoints.RemoveAt(accessPoints.Count - 1);
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
            if (state.IsJustLost || state.IsGoingBack) { return; }
            var dir = agent.nextPosition - character.transform.position;
            // 位置の更新
            character.transform.position = agent.nextPosition;
            character.transform.rotation = currentRotation;
        }
    }

    public class StraightTrackingHabitException : System.Exception {
        public StraightTrackingHabitException(string message) : base(message) { }
    }
}