using UnityEngine;
using GFWK.Internal;
using GFWK.Internal.Structures;
using GFWK.Runtime.UI.Bindings;

namespace GFWK.Runtime.UI
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
        void OnLocalNotification(GFWKLocalNotification notification)
        {
            listHandler.Initialize();
            listHandler.InstatiateAndGet<UILeftNotifier>().SetInfo(notification.Message, showTime);
        }
    }
}