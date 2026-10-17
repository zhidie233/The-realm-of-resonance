using UnityEngine;
using System.Collections.Generic;
using Game.Internal.Structures;

public class LocalKillNotifier : MonoBehaviour
{
    [Range(1, 7)] public float IndividualShowTime = 3;
    public GameObject multipleNotifierRoot;
    public RectTransform panelRect;
    public GameObject notificationTemplate;
    public LocalKillUI staticNotification;

    private List<KillInfo> localKillsQueque = new List<KillInfo>();

    private void Start()
    {
        multipleNotifierRoot.SetActive(GameConfig.Instance.localKillsShowMode == LocalKillDisplay.List);
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

    void OnLocalKill(KillInfo info)
    {
        if (GameConfig.Instance.localKillsShowMode == LocalKillDisplay.List) InstanceSingleNotification(info);
        else if (GameConfig.Instance.localKillsShowMode == LocalKillDisplay.Queqe) ShowNotification(info);
    }

    void OnLocalNotification(GameLocalNotification notification)
    {

    }

    void InstanceSingleNotification(KillInfo info)
    {
        GameObject newkillfeedh = Instantiate(notificationTemplate) as GameObject;
        newkillfeedh.GetComponent<LocalKillUI>().InitMultiple(info, info.byHeadShot);
        notificationTemplate.SetActive(true);
        newkillfeedh.transform.SetParent(panelRect, false);
        newkillfeedh.transform.SetAsFirstSibling();
        notificationTemplate.SetActive(false);
    }

    void ShowNotification(KillInfo info)
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

    private static LocalKillNotifier _instance;
    public static LocalKillNotifier Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<LocalKillNotifier>(); }
            return _instance;
        }
    }

    [System.Serializable]
    public enum LocalKillDisplay
    {
        Queqe,
        List,
        Custom,
    }
}