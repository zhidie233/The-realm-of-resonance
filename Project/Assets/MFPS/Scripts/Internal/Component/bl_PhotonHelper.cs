using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class bl_PhotonHelper : MonoBehaviourPun {

    protected GameMode mGameMode = GameMode.FFA;
    private List<Player> PlayerList = new List<Player>();
    private bool GameModeDownloaded = false;

    public string myTeam
    {
        get
        {
            string t = (string)bl_PhotonNetwork.LocalPlayer.CustomProperties[PropertiesKeys.TeamKey];
            return t;
        }
    }

    public bool isRoomReady
    {
        get { return (bl_PhotonNetwork.IsConnected && bl_PhotonNetwork.InRoom); }
    }

    // Find a player gameobject by the viewID
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
    // get a photonView by the viewID
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

    // <param name="go"></param>
    // <returns></returns>
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

    // True if the PhotonView is "mine" and can be controlled by this client.
    // <remarks>
    // PUN has an ownership concept that defines who can control and destroy each PhotonView.
    // True in case the owner matches the local PhotonPlayer.
    // True if this is a scene photon view on the Master client.
    // </remarks>
    public bool isMine
    {
        get
        {
            return photonView.IsMine;
        }
    }

    // Get Photon.connect
    public bool isConnected
    {
        get
        {
            return PhotonNetwork.IsConnected;
        }
    }

    // <returns></returns>
    public GameObject FindPhotonPlayer(Player p)
    {
        GameObject player = GameObject.Find(p.NickName);
        if (player == null)
        {
            return null;
        }
          return player;
    }

    // Get the team of players
    // <param name="p"></param>
    // <returns></returns>
    public string GetTeam(Player p)
    {
        if (p == null || !isConnected)
            return null;

            return (string)p.CustomProperties[PropertiesKeys.TeamKey];
    }

    // Get the team of players
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

    // Get current gamemode
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

    // Get current gamemode
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
            if (bl_PhotonNetwork.LocalPlayer != null && isConnected)
            {
                return bl_PhotonNetwork.LocalPlayer.NickName;
            }
            else
            {
                return "None";
            }
        }
    }

    // Get All Player in Room of a specific team
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

    // is the a one team game mode?
    public bool isOneTeamMode
    {
        get
        {
            bool b = false;
            GameMode m = GetGameMode;
            if (m == GameMode.FFA) { b = true; }
#if GR
            if (m == GameMode.GR) { b = true; }
#endif
#if LMS
            if (m == GameMode.BR) { b = true; }
#endif
            return b;
        }
    }

    // is the a one team game mode?
    public bool isOneTeamModeUpdate
    {
        get
        {
            bool b = false;
            GameMode m = GetGameModeUpdated;
            if (m == GameMode.FFA) { b = true; }
#if GR
            if (m == GameMode.GR) { b = true; }
#endif
#if LMS
            if (m == GameMode.BR) { b = true; }
#endif
            return b;
        }
    }

    public Player LocalPlayer => bl_PhotonNetwork.LocalPlayer;
}