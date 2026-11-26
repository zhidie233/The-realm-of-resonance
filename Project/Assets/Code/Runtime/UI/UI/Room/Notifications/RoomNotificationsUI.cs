using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
namespace Resonance.UI
{

    public class RoomNotificationsUI : MonoBehaviour
    {
        public ListLayoutHelper listHandler;
        public float showTime = 5;

        private void OnEnable()
        {
            listHandler.Prefab.SetActive(false);
            EventBus.Subscribe<LocalNotificationEvent>(OnEvt_onLocalNotification_OnLocalNotification);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocalNotificationEvent>(OnEvt_onLocalNotification_OnLocalNotification);
        }

        void OnLocalNotification(GameLocalNotification notification)
        {
            listHandler.Initialize();
            listHandler.InstantiateAndGet<UILeftNotifier>().SetInfo(notification.Message, showTime);
        }

        private void OnEvt_onLocalNotification_OnLocalNotification(LocalNotificationEvent e) => OnLocalNotification(e.Notification);
    }
}
