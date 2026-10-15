﻿using Game.Runtime.AI;
using Photon.Realtime;
using UnityEngine;

public abstract class PlayerScoreboardTableBase : MonoBehaviour
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="uiPrefab"></param>
    /// <returns></returns>
    public abstract PlayerScoreboardUIBase Instance(Player player, GameObject uiPrefab);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="uiPrefab"></param>
    /// <returns></returns>
    public abstract PlayerScoreboardUIBase InstanceBot(GameBotProperties player, GameObject uiPrefab);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActiveJoinButton(bool active);

    /// <summary>
    /// 
    /// </summary>
    public abstract void ResetJoinButton();
}