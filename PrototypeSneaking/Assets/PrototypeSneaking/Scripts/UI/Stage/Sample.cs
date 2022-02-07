using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PrototypeSneaking.UI.Stage
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private GameObject button;
        void Awake()
        {
            AddEventTrigger(button, OnClick);
        }

        public void AddEventTrigger(GameObject gameObj, Action action)
        {
            EventTrigger currentTrigger = gameObj.AddComponent<EventTrigger>();
            currentTrigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((x) => action());
            currentTrigger.triggers.Add(entry);
        }

        public void OnClick()
        {
        }
    }
}