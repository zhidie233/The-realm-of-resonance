using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Game.Runtime.AI;

[Serializable]
public class GamePlayer
{
    public string Name;
    public Transform Actor;
    public bool isRealPlayer = true;
    public bool isAlive = true;
    public Team Team = Team.None;
    public Transform AimPosition;

    public PhotonView m_actorView;
    // 此角色的 PhotonView
    public PhotonView ActorView
    {
        get => m_actorView;
        set
        {
            m_actorView = value;
            if (m_actorView != null) ActorViewID = m_actorView.ViewID;
        }
    }

    private int _viewID;
    // 此角色的 PhotonView ID
    public int ActorViewID
    {
        get => _viewID;
        set => _viewID = value;
    }

    // 此角色的 Photon 玩家 Actor Number
    public int ActorNumber
    {
        get
        {
            if (ActorView == null || ActorView.Owner == null) return -1;
            return ActorView.Owner.ActorNumber;
        }
    }

    public GameBotProperties GetBotStats()
    {
        GameBotProperties botProps = AIMananger.Instance.GetBotStatistics(Name);
        if (botProps == null)
        {
            Debug.LogWarning($"Bot {Name} not found in AI Manager, this could be a de-sync issue.");
            //返回空容器以防报错
            botProps = new GameBotProperties();
            botProps.Name = Name;
        }
        return botProps;
    }

    public Player GetNetworkPlayer()
    {
        if (ActorView != null) return ActorView.Owner;
        else
        {
            var playerList = PhotonNetworkWrapper.PlayerList;
            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i].NickName == Name) return playerList[i];
            }
            return null;
        }      
    }

    // 获取真实玩家或机器人的属性值
    // <param name="key"></param>
    // <param name="defaultValue"></param>
    // <returns></returns>
    public object GetPlayerPropertie(string key, object defaultValue = null)
    {
        switch (key)
        {
            case PropertiesKeys.KillsKey:
                if (isRealPlayer) return GetNetworkPlayer().GetKills();
                else return GetBotStats().Kills;
            case PropertiesKeys.DeathsKey:
                if (isRealPlayer) return GetNetworkPlayer().GetDeaths();
                else return GetBotStats().Deaths;
            case PropertiesKeys.ScoreKey:
                if (isRealPlayer) return GetNetworkPlayer().GetPlayerScore();
                else return GetBotStats().Score;
            default:
                Debug.LogWarning($"Property {key} has not been setup yet.");
                return defaultValue;
        }
    }

    public GamePlayer() { }

    public GamePlayer(PhotonView view, bool realPlayer = true, bool alive = true)
    {
        BuildFromView(view, realPlayer, alive);
    }

    public GamePlayer BuildFromView(PhotonView view, bool realPlayer = true, bool alive = true)
    {
        isRealPlayer = realPlayer;
        isAlive = alive;
        if (view == null) return this;

        Actor = view.transform;
        ActorView = view;
        AimPosition = Actor;
        if (view.InstantiationData != null)
        {
            if (!isRealPlayer)
            {
                Name = (string)view.InstantiationData[0];
                Team = (Team)view.InstantiationData[1];
            }
            else
            {
                Name = view.Owner.NickName;
                Team = (Team)view.InstantiationData[0];
            }
        }
        return this;
    }
}