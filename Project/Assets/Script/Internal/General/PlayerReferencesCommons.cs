using Photon.Pun;
using System;
using UnityEngine;

public abstract class PlayerReferencesCommon : MonoBehaviour
{
    /// 此玩家死亡时调用
    public Action onDie;

    public abstract Collider[] AllColliders
    {
        get;
    }

    public abstract Animator PlayerAnimator
    {
        get;
        set;
    }

    public abstract Transform BotAimTarget
    {
        get;
        set;
    }

    public abstract Team PlayerTeam
    {
        get;
    }

    private PhotonView _networkView = null;
    /// 玩家网络视图
    public PhotonView NetworkView
    {
        get
        {
            if (_networkView == null) _networkView = GetComponent<PhotonView>();
            return _networkView;
        }
    }

    public abstract void IgnoreColliders(Collider[] list, bool ignore);
    /// 判断此玩家是否死亡
    /// 因为玩家死亡后会以布娃娃形式短暂存在于游戏中
    public abstract bool IsDeath();
    /// 获取此玩家的生命值
    public abstract int GetHealth();
    /// 获取此玩家的最大生命值
    public virtual int GetMaxHealth() { return 100; }
    /// 是否为本地玩家？
    public virtual bool IsLocal()
    {
        return NetworkView.IsMine;
    }
    /// 此玩家是真实玩家还是机器人？
    public virtual bool IsRealPlayer()
    {
        return true;
    }
    /// 此玩家是否与本地玩家同队？
    public virtual bool IsTeamMateOfLocalPlayer()
    {
        return RoomSettings.Instance != null && RoomSettings.Instance.isOneTeamMode ? false : PlayerTeam == GameCore.LocalPlayer.Team;
    }
}
