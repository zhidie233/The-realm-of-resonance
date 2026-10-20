using UnityEngine;
using System.Collections.Generic;

public class KillNotifier : MonoBehaviour
{
    [Range(1, 7)] public float IndividualShowTime = 3;
    public GameObject multipleNotifierRoot;
    public RectTransform panelRect;
    public GameObject notificationTemplate;
    public KillNoticeUI staticNotification;

    private List<KillRecord> localKillsQueque = new List<KillRecord>();

    private void Start()
    {
        multipleNotifierRoot.SetActive(GameConfig.Instance.localKillsShowMode == KillNotice.List);
    }

    private void OnEnable()
    {
        GameEvents.onLocalKill += OnLocalKill;
        GameEvents.onLocalNotification += OnLocalNotification;
    }

    private void OnDisable()
    {
        GameEvents.onLocalKill -= OnLocalKill;
        GameEvents.onLocalNotification -= OnLocalNotification;
    }

    void OnLocalKill(KillRecord info)
    {
        if (GameConfig.Instance.localKillsShowMode == KillNotice.List) InstanceSingleNotification(info);
        else if (GameConfig.Instance.localKillsShowMode == KillNotice.Queqe) ShowNotification(info);
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
        if (localKillsQueque.Count <= 0)
        {
            staticNotification.InitIndividual(info);
        }
        localKillsQueque.Add(info);
    }

    public void LocalDisplayDone()
    {
        localKillsQueque.RemoveAt(0);
        if (localKillsQueque.Count > 0)
        {
            staticNotification.InitIndividual(localKillsQueque[0]);
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
        Queqe,
        List,
        Custom,
    }
}