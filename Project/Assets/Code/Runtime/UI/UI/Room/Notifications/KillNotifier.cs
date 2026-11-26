using UnityEngine;
using System.Collections.Generic;
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

    public class KillNotifier : MonoBehaviour
    {
        [Range(1, 7)] public float IndividualShowTime = 3;
        public GameObject multipleNotifierRoot;
        public RectTransform panelRect;
        public GameObject notificationTemplate;
        public KillNoticeUI staticNotification;

        private List<KillRecord> localKillsQueue = new List<KillRecord>();

        private void Start()
        {
            multipleNotifierRoot.SetActive(ProjectConfigRoot.Instance.personalKillFeedDisplay == KillNotice.List);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LocalKillEvent>(OnEvt_onLocalKill_OnLocalKill);
            EventBus.Subscribe<LocalNotificationEvent>(OnEvt_onLocalNotification_OnLocalNotification);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocalKillEvent>(OnEvt_onLocalKill_OnLocalKill);
            EventBus.Unsubscribe<LocalNotificationEvent>(OnEvt_onLocalNotification_OnLocalNotification);
        }

        void OnLocalKill(KillRecord info)
        {
            if (ProjectConfigRoot.Instance.personalKillFeedDisplay == KillNotice.List) InstanceSingleNotification(info);
            else if (ProjectConfigRoot.Instance.personalKillFeedDisplay == KillNotice.Queue) ShowNotification(info);
        }

        void OnLocalNotification(GameLocalNotification notification)
        {

        }

        void InstanceSingleNotification(KillRecord info)
        {
            GameObject newkillfeedh = Instantiate(notificationTemplate) as GameObject;
            newkillfeedh.GetComponent<KillNoticeUI>().InitMultiple(info, info.byHeadShot);
            notificationTemplate.SetActive(true);
            newkillfeedh.transform.SetParent(panelRect, false);
            newkillfeedh.transform.SetAsFirstSibling();
            notificationTemplate.SetActive(false);
        }

        void ShowNotification(KillRecord info)
        {
            if (localKillsQueue.Count <= 0)
            {
                staticNotification.InitIndividual(info);
            }
            localKillsQueue.Add(info);
        }

        public void LocalDisplayDone()
        {
            localKillsQueue.RemoveAt(0);
            if (localKillsQueue.Count > 0)
            {
                staticNotification.InitIndividual(localKillsQueue[0]);
            }
        }

        private static KillNotifier _instance;
        public static KillNotifier Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<KillNotifier>(); }
                return _instance;
            }
        }

        [System.Serializable]
        public enum KillNotice
        {
            Queue,
            List,
            Custom,
        }

        private void OnEvt_onLocalKill_OnLocalKill(LocalKillEvent e) => OnLocalKill(e.Record);
        private void OnEvt_onLocalNotification_OnLocalNotification(LocalNotificationEvent e) => OnLocalNotification(e.Notification);
    }
}
