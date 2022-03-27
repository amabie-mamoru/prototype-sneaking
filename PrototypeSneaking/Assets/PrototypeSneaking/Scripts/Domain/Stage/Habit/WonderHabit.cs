using System.Collections.Generic;
using UnityEngine;

namespace PrototypeSneaking.Domain.Stage
{
    public class WonderHabit : CharacterHabit
    {
        [SerializeField] private List<Vector3> wonderingPaths;
        private int wonderingVecterSign = 1;
        private int wonderingNextPointIndex = 1;

        [SerializeField] private float rotationSpeed = 0.5f;
        private float rotatingElapsedRatio;
        private bool beginToRotate;
        private Quaternion capturedBeginningQuaternion;
        private float ROTATION_ALLOWABLE_ERROR = 20f; // NOTE: 現状だと動いてる間にも 1.27 くらいは動くことがあるので

        private bool beginToMove;
        private const float MOVE_ALLOWABLE_ERROR = 0.08f; // NOTE: 現状だと 0.0666... に収束してしまったので少し大きめの値に設定

        public override void Behave()
        {
            DecideDirection();
            if (ShouldRotate())
            {
                Rotate();
            }
            else
            {
                Move();
            }
        }

        /// <summary>
        /// 進む向きを決める
        /// </summary>
        private void DecideDirection()
        {
            // 到達点にきてなかったら何もしない
            if (Vector3.Magnitude(character.transform.position - wonderingPaths[wonderingNextPointIndex]) > MOVE_ALLOWABLE_ERROR) { return; }
            if (wonderingNextPointIndex == 0) { wonderingVecterSign = 1; }
            if (wonderingNextPointIndex == wonderingPaths.Count - 1) { wonderingVecterSign = -1; }
            wonderingNextPointIndex += wonderingVecterSign;
        }

        private bool ShouldRotate()
        {
            Vector3 direction = wonderingPaths[wonderingNextPointIndex] - character.transform.position;
            var targetQuaternion = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            var shouldRotate = Vector3.Magnitude(targetQuaternion.eulerAngles - character.transform.rotation.eulerAngles) > ROTATION_ALLOWABLE_ERROR;
            if (!shouldRotate) {
                beginToRotate = false;
            }
            return shouldRotate;
        }

        private void Rotate()
        {
            if (!beginToRotate)
            {
                rotatingElapsedRatio = 0f;
                capturedBeginningQuaternion = character.transform.rotation;
                beginToRotate = true;
            }
            Vector3 direction = wonderingPaths[wonderingNextPointIndex] - character.transform.position;
            var targetQuaternion = Quaternion.LookRotation(direction, Vector3.up);
            rotatingElapsedRatio += rotationSpeed * Time.deltaTime;

            // ここにくることはほぼない。なぜなら、ShouldRotate で早めに回転をやめてしまうから
            if (rotatingElapsedRatio >= 1f)
            {
                rotatingElapsedRatio = 1;
                beginToRotate = false;
            }
            character.transform.rotation = Quaternion.Lerp(capturedBeginningQuaternion, targetQuaternion, rotatingElapsedRatio);
        }

        private void Move()
        {
            if (!beginToMove)
            {
                agent.SetDestination(wonderingPaths[wonderingNextPointIndex]);
                beginToMove = true;
            }
            character.transform.position = agent.nextPosition;
            if (Vector3.Magnitude(character.transform.position - wonderingPaths[wonderingNextPointIndex]) <= MOVE_ALLOWABLE_ERROR) {
                agent.SetDestination(character.transform.position);
                beginToMove = false;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (wonderingPaths != null && wonderingPaths.Count > 1)
            {
                Gizmos.color = Color.blue;
                Vector3 prevPos = wonderingPaths[0];
                for (int i = 1; i < wonderingPaths.Count; i++)
                {
                    Gizmos.DrawLine(prevPos, wonderingPaths[i]);
                    prevPos = wonderingPaths[i];
                }
            }
        }
#endif
    }
}