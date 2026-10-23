using System;
using UnityEngine;

public abstract class ObserverModeBase : MonoBehaviourBase
{
    [Serializable]
    public enum LeaveSpectatorModeAction
    {
        AllowSelectTeam,
        ReturnToLobby,
    }
    /// 观战模式是否处于激活状态？
    public virtual bool isActive 
    { 
        get; 
        set; 
    } = false;
    /// 本地玩家是否以观战者身份进入了当前对局？
    private static bool s_enterAsSpectator = false;
    public static bool EnterAsSpectator
    {
        get => s_enterAsSpectator;
        set => s_enterAsSpectator = value;
    }

    public abstract void SetActiveSpectatorMode(bool active);

    public abstract void SetActiveUI(bool active);
    /// 玩家能否在进入观战模式后选择队伍？
    public virtual bool CanSelectTeam()
    {
        if (!isActive) return true;
        return GameConfig.Instance.onLeaveSpectatorMode == LeaveSpectatorModeAction.AllowSelectTeam;
    }

    private static ObserverModeBase _instance;
    public static ObserverModeBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ObserverModeBase>();
            }
            return _instance;
        }
    }
}
