using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PrototypeSneaking.Domain.Stage
{
    public class Sensor : MonoBehaviour
    {
        // NOTE: 開発効率と処理速度を天秤にかけてあえて Vector3 で保持するのはやめた
        // 処理速度優先なら GameObject を配置しない方が効率はいいはずだが
        // 本体の Translate / Rotate が発生した場合に Vector3 だとそちらにも処理をしないといけない上
        // UnityEngine インターフェースで予想外の処理を書いてしまった時のバグ検知リスクの方が高いと考えて
        // あえて GameObject で表現するようにした
        // 管理できる仕組み作りにできる自身があるなら Vector3 の方がよい
        [SerializeField] List<GameObject> eyes;
        [SerializeField] List<GameObject> edges;

        public List<Vector3> Eyes {
            get {
                // NOTE: キャッシュしちゃうと移動したときに座標が古いものを参照するのでキャッシュできない
                List<Vector3>  _eyes = new List<Vector3>();
                foreach (var eye in eyes)
                {
                    _eyes.Add(eye.transform.position);
                }
                return _eyes;
            }
        }
        public List<Vector3> Edges {
            get {
                List<Vector3>  _edges = new List<Vector3>();
                foreach (var edge in edges)
                {
                    _edges.Add(edge.transform.position);
                }
                return _edges;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Sensor))]
    public class SensorEditor : Editor
    {
        public void OnEnable()
        {
            var eyes = serializedObject.FindProperty("eyes");
            var edges = serializedObject.FindProperty("edges");
            for (var i = 0; i < eyes.arraySize; i++)
            {
                if (eyes.GetArrayElementAtIndex(i).objectReferenceValue == null) { continue; }
                MakePoint((GameObject)eyes.GetArrayElementAtIndex(i).objectReferenceValue, "Eye");
            }
            for (var i = 0; i < edges.arraySize; i++)
            {
                if (edges.GetArrayElementAtIndex(i).objectReferenceValue == null) { continue; }
                MakePoint(edges.GetArrayElementAtIndex(i).objectReferenceValue as GameObject, "Edge");
            }
        }

        private void MakePoint(GameObject gameObj, string prefabName)
        {
            var sensor = Transform.FindObjectOfType<Sensor>();
            // Prefab を Scene Window で見た時は null。 Scene 上に配置したときは not null になる
            if (sensor == null) { return; }
            // 本来は以下の方がいいらしいがなぜか null になるので。
            //var prefab = EditorGUIUtility.Load("Prefabs/EditorPoint") as GameObject;
            var prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
            var point = Instantiate<GameObject>(prefab);
            point.tag = "DebugEditor";
            point.name = point.name.Replace("(Clone)", "");
            point.transform.SetParent(sensor.transform);
            point.transform.position = gameObj.transform.position;
        }

        public void OnDisable()
        {
            var gameObjects = GameObject.FindGameObjectsWithTag("DebugEditor");
            foreach (var gameObj in gameObjects) {
                DestroyImmediate(gameObj);
            }
        }
    }
#endif
}