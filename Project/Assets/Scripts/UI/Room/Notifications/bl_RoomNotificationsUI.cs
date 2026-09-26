using UnityEngine;
using GFWK.Internal;
using GFWK.Internal.Structures;
using GFWK.Runtime.UI.Bindings;

namespace GFWK.Runtime.UI
{
    public class bl_RoomNotificationsUI : MonoBehaviour
    {
        public UIListHandler listHandler;
        public float showTime = 5;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            listHandler.Prefab.SetActive(false);
            bl_EventHandler.onLocalNotification += OnLocalNotification;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onLocalNotification -= OnLocalNotification;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalNotification(GFWKLocalNotification notification)
        {
            listHandler.Initialize();
            listHandler.InstatiateAndGet<bl_UILeftNotifier>().SetInfo(notification.Message, showTime);
        }
    }
}