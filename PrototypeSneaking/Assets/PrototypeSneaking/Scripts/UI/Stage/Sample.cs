using System;
using System.Collections.Generic;
using PrototypeSneaking.Domain;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PrototypeSneaking.UI.Stage
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private GameObject button;
        [SerializeField] private Character character;

        void Awake()
        {
            AddEventTrigger(button, OnClick);
            Application.Stage.CharacterController.Instance.Attach(character);
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