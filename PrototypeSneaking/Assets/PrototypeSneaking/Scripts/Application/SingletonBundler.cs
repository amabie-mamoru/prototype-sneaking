using UnityEngine;
using System;

namespace PrototypeSneaking.Application
{
    public class SingletonBundler : MonoBehaviour
    {
        private void Awake()
        {
            // フレームレートの設定
            UnityEngine.Application.targetFrameRate = 60;
            MakeSingletonMonoBehavoiur("Stage.CharacterController");
        }

        private void MakeSingletonMonoBehavoiur(string name)
        {
            var gameObj = new GameObject();
            gameObj.name = name;
            gameObj.AddComponent(ApplicationName(name));
        }

        private Type ApplicationName(string name)
        {
            return Type.GetType($"PrototypeSneaking.Application.{name}");
        }
    }
}