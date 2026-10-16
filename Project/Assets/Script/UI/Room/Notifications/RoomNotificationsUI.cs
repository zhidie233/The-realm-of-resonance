﻿using UnityEngine;
using Game.Internal;
using Game.Internal.Structures;
using Game.Runtime.UI.Bindings;

namespace Game.Runtime.UI
{
    public class RoomNotificationsUI : MonoBehaviour
    {
        public UIListHandler listHandler;
        public float showTime = 5;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            listHandler.Prefab.SetActive(false);
            GameEvents.onLocalNotification += OnLocalNotification;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            GameEvents.onLocalNotification -= OnLocalNotification;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalNotification(GameLocalNotification notification)
        {
            listHandler.Initialize();
            listHandler.InstatiateAndGet<UILeftNotifier>().SetInfo(notification.Message, showTime);
        }
    }
}