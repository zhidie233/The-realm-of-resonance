﻿using UnityEngine;
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

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        multipleNotifierRoot.SetActive(GameConfig.Instance.localKillsShowMode == LocalKillDisplay.List);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        GameEvents.onLocalKill += OnLocalKill;
        GameEvents.onLocalNotification += OnLocalNotification;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        GameEvents.onLocalKill -= OnLocalKill;
        GameEvents.onLocalNotification -= OnLocalNotification;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void OnLocalKill(KillInfo info)
    {
        if (GameConfig.Instance.localKillsShowMode == LocalKillDisplay.List) InstanceSingleNotification(info);
        else if (GameConfig.Instance.localKillsShowMode == LocalKillDisplay.Queqe) ShowNotification(info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="notification"></param>
    void OnLocalNotification(GameLocalNotification notification)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void InstanceSingleNotification(KillInfo info)
    {
        GameObject newkillfeedh = Instantiate(notificationTemplate) as GameObject;
        newkillfeedh.GetComponent<LocalKillUI>().InitMultiple(info, info.byHeadShot);
        notificationTemplate.SetActive(true);
        newkillfeedh.transform.SetParent(panelRect, false);
        newkillfeedh.transform.SetAsFirstSibling();
        notificationTemplate.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void ShowNotification(KillInfo info)
    {
        if (localKillsQueque.Count <= 0)
        {
            staticNotification.InitIndividual(info);
        }
        localKillsQueque.Add(info);
    }

    /// <summary>
    /// 
    /// </summary>
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