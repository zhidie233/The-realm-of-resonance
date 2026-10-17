using UnityEngine;
using Game.Internal;
using Game.Internal.Structures;
using Game.Runtime.UI.Bindings;

namespace Game.Runtime.UI
{
    public class RoomNotificationsUI : MonoBehaviour
    {
        public UIListHandler listHandler;
        public float showTime = 5;

        private void OnEnable()
        {
            listHandler.Prefab.SetActive(false);
            GameEvents.onLocalNotification += OnLocalNotification;
        }

        private void OnDisable()
        {
            GameEvents.onLocalNotification -= OnLocalNotification;
        }

        void OnLocalNotification(GameLocalNotification notification)
        {
            listHandler.Initialize();
            listHandler.InstatiateAndGet<UILeftNotifier>().SetInfo(notification.Message, showTime);
        }
    }
}