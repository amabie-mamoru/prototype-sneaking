using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PrototypeSneaking.Domain
{
    public class Sensor : MonoBehaviour
    {
        [SerializeField] List<Vector3> eyes;
        [SerializeField] List<Vector3> edges;

        private Edges _edges;
        public List<Vector3> Eyes => eyes;
        public Edges Edges {
            get {
                if (_edges == null) {
                    var __edges = new Edge[edges.Count];
                    for (int i = 0; i < edges.Count; i++)
                    {
                        __edges[i] = new Edge(edges[i]);
                    }
                    _edges = new Edges(__edges);
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
                MakePoint(eyes.GetArrayElementAtIndex(i).vector3Value, "Eye");
            }
            for (var i = 0; i < edges.arraySize; i++)
            {
                MakePoint(edges.GetArrayElementAtIndex(i).vector3Value, "Edge");
            }
        }

        private void MakePoint(Vector3 pos, string prefabName)
        {
            var sensor = Transform.FindObjectOfType<Sensor>();
            // 本来は以下の方がいいらしいがなぜか null になるので。
            //var prefab = EditorGUIUtility.Load("Prefabs/EditorPoint") as GameObject;
            var prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
            var point = Instantiate<GameObject>(prefab);
            point.tag = "DebugEditor";
            point.name = point.name.Replace("(Clone)", "");
            point.transform.SetParent(sensor.transform);
            point.transform.position = pos;
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