using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class PhotonHelper : MonoBehaviourPun {

    protected GameMode mGameMode = GameMode.FFA;
    private List<Player> PlayerList = new List<Player>();
    private bool GameModeDownloaded = false;

    public string myTeam
    {
        get
        {
            string t = (string)PhotonNetworkWrapper.LocalPlayer.CustomProperties[PropertiesKeys.TeamKey];
            return t;
        }
    }

    public bool isRoomReady
    {
        get { return (PhotonNetworkWrapper.IsConnected && PhotonNetworkWrapper.InRoom); }
    }

    // 按 ViewID 查找玩家 GameObject
    // <returns></returns>
    public GameObject FindPlayerRoot(int view)
    {
        PhotonView _view = PhotonView.Find(view);
        if (_view != null)
        {
            return _view.gameObject;
        }
        else
        {
            return null;
        }
    }
    // 按 ViewID 获取 PhotonView
    // <param name="view"></param>
    // <returns></returns>
    public PhotonView FindPlayerView(int view)
    {
        PhotonView _view = PhotonView.Find(view);

        if (_view != null)
        {
            return _view;
        }
        else
        {
            return null;
        }
    }

    public void CheckViewAllocation()
    {
        if (PhotonNetwork.IsMasterClient && photonView.ViewID <= 0)
        {
            PhotonNetwork.AllocateRoomViewID(photonView);
        }
    }

    public PhotonView GetPhotonView(GameObject go)
    {
        PhotonView view = go.GetComponent<PhotonView>();
        if (view == null)
        {
            view = go.GetComponentInChildren<PhotonView>();
        }
        return view;
    }
    public Transform Root
    {
        get
        {
            return transform.root;
        }
    }

    public Transform Parent
    {
        get
        {
            return transform.parent;
        }
    }

    // 当 PhotonView 属于本客户端并可被其控制时为 true。
    // <remarks>
    // PUN 有所有权概念，规定谁能控制和销毁各 PhotonView。
    // 所有者与本地 PhotonPlayer 一致时为 true。
    // 这是主客户端上的场景 PhotonView 时为 true。
    // </remarks>
    public bool isMine
    {
        get
        {
            return photonView.IsMine;
        }
    }

    // 获取 Photon 连接
    public bool isConnected
    {
        get
        {
            return PhotonNetwork.IsConnected;
        }
    }

    public GameObject FindPhotonPlayer(Player p)
    {
        GameObject player = GameObject.Find(p.NickName);
        if (player == null)
        {
            return null;
        }
          return player;
    }

    // 获取玩家所属队伍
    // <param name="p"></param>
    // <returns></returns>
    public string GetTeam(Player p)
    {
        if (p == null || !isConnected)
            return null;

            return (string)p.CustomProperties[PropertiesKeys.TeamKey];
    }

    // 获取玩家所属队伍
    // <param name="p"></param>
    // <returns></returns>
    public Team GetTeamEnum(Player p)
    {
        if (p == null || !isConnected)
            return Team.All;

        string t = (string)p.CustomProperties[PropertiesKeys.TeamKey];
        
        switch (t)
        {
            case "Team2":
                return Team.Team2;
            case "Team1":
                return Team.Team1;
        }
        return Team.All;
    }

    // 获取当前游戏模式
    public GameMode GetGameMode
    {
        get
        {
            if (!isConnected || !PhotonNetwork.InRoom)
                return GameMode.FFA;

            if (!GameModeDownloaded)
            {
                GameMode[] result = (GameMode[])System.Enum.GetValues(typeof(GameMode));
                string gm = (string)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.GameModeKey];
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i].ToString() == gm)
                    {
                        mGameMode = result[i];
                        GameModeDownloaded = true;
                        break;
                    }
                }
            }

            return mGameMode;
        }
    }

    // 获取当前游戏模式
    public GameMode GetGameModeUpdated
    {
        get
        {
            if (!isConnected || !PhotonNetwork.InRoom)
                return GameMode.FFA;

            GameMode[] result = (GameMode[])System.Enum.GetValues(typeof(GameMode));
            string gm = (string)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.GameModeKey];
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i].ToString() == gm)
                {
                    return result[i];
                }
            }
            return mGameMode;
        }
    }

    public string LocalName
    {
        get
        {
            if (PhotonNetworkWrapper.LocalPlayer != null && isConnected)
            {
                return PhotonNetworkWrapper.LocalPlayer.NickName;
            }
            else
            {
                return "None";
            }
        }
    }

    // 获取房间内指定队伍的所有玩家
    // <param name="team"></param>
    // <returns></returns>
    public List<Player> GetPlayersInTeam(string team)
    {
        PlayerList.Clear();
        PlayerList = new List<Player>();

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if ((string)players[i].CustomProperties[PropertiesKeys.TeamKey] == team)
            {
                PlayerList.Add(players[i]);
            }
        }
        return PlayerList;
    }

    // 是否为单队伍游戏模式？
    public bool isOneTeamMode
    {
        get
        {
            bool b = false;
            GameMode m = GetGameMode;
            if (m == GameMode.FFA) { b = true; }
            return b;
        }
    }

    // 是否为单队伍游戏模式？
    public bool isOneTeamModeUpdate
    {
        get
        {
            bool b = false;
            GameMode m = GetGameModeUpdated;
            if (m == GameMode.FFA) { b = true; }
            return b;
        }
    }

    public Player LocalPlayer => PhotonNetworkWrapper.LocalPlayer;
}