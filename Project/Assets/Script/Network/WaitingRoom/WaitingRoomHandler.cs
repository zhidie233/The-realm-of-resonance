using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class WaitingRoomHandler : WaitingRoomBase, IMatchmakingCallbacks, IInRoomCallbacks
{
    public List<int> readyPlayers = new List<int>();
    static readonly RaiseEventOptions EventsAll = new RaiseEventOptions();

    private bool isLocalReady = false;

    void Awake()
    {
        readyPlayers.Clear();
        PhotonNetwork.AddCallbackTarget(this);
        EventsAll.Receivers = ReceiverGroup.All;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventCustom;
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventCustom;
    }

    public void AutoJoinToTeam()
    {
        if (isOneTeamModeUpdate)
        {
            JoinToTeam(Team.All);
        }
        else
        {
            int team1Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
            int team2Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
            if (team1Count > team2Count) { JoinToTeam(Team.Team2); }
            else if (team1Count < team2Count) { JoinToTeam(Team.Team1); }
            else { JoinToTeam(Team.Team1); }
        }
    }

    public override void JoinToTeam(Team team)
    {
        NetworkWrapper.LocalPlayer.SetPlayerTeam(team);
        ObserverModeBase.EnterAsSpectator = false;
    }

    void OnLocalEnterInWaitingRoom()
    {
        var table = CommonUtil.CreatePhotonHashTable();
        table.Add(NetworkKeys.UserRole, GameConfig.Instance.RolePrefix);
        table.Add(NetworkKeys.PlayerTotalScore, GameCore.LocalPlayer.Stats.GetAllTimeScore());
        NetworkWrapper.LocalPlayer.SetCustomProperties(table);
    }

    /// <summary>
    /// 本地
    /// </summary>
    public override void SetLocalPlayerReady()
    {
        isLocalReady = !isLocalReady;
        HashTable table = new HashTable();
        table.Add(NetworkKeys.WaitingState, isLocalReady ? (int)PlayerState.Ready : (int)PlayerState.NoReady);
        table.Add(NetworkKeys.PlayerID, NetworkWrapper.LocalPlayer.ActorNumber);
        PhotonNetwork.RaiseEvent(NetworkKeys.WaitingPlayerReadyEvent, table, EventsAll, SendOptions.SendReliable);
    }

    /// <summary>
    /// 来自服务器
    /// </summary>
    void OnReceivePlayerState(HashTable data)
    {
        if (!data.ContainsKey(NetworkKeys.WaitingState)) return;

        PlayerState state = (PlayerState)(int)data[NetworkKeys.WaitingState];
        int playerID = (int)data[NetworkKeys.PlayerID];
        if (state == PlayerState.Ready)
        {
            if (!readyPlayers.Contains(playerID)) { readyPlayers.Add(playerID); }
        }
        else
        {
            if (readyPlayers.Contains(playerID)) { readyPlayers.Remove(playerID); }
        }
        OnUpdateStates();
    }

    /// <summary>
    /// 主客户端点击开始按钮后调用
    /// </summary>
    public override void StartGame()
    {
        if (!NetworkWrapper.IsMasterClient) return;

        SetWaitingState(WaitingState.Started);
    }

    void SetWaitingState(WaitingState state)
    {
        HashTable table = new HashTable();
        table.Add(NetworkKeys.WaitingState, state);

        HashTable expectTable = new HashTable(1);
        object temp;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(NetworkKeys.WaitingState, out temp))
        {
            expectTable[NetworkKeys.WaitingState] = temp;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(table, expectTable);
        //不需要用 RPC，因为所有玩家都会收到 OnRoomPropertiesUpdate 回调，用它就行
    }

    void OnUpdateStates()
    {
        WaitingRoomUIBase.Instance.UpdateAllPlayersStates();
        if (NetworkWrapper.IsMasterClient)
        {

        }
    }

    void OnReceiveInitialInfo(HashTable data)
    {
        string source = (string)data[NetworkKeys.PlayerID];
        int[] split = source.Split(","[0]).Select(x => int.Parse(x)).ToArray();
        readyPlayers.AddRange(split);
        OnUpdateStates();
    }

    /// <summary>
    /// RaiseEvent 相当于 RPC，这里用它只是因为我更喜欢 :)
    /// </summary>
    public void OnEventCustom(EventData data)
    {
        if (data.CustomData == null) return;

        HashTable t = (HashTable)data.CustomData;
        switch (data.Code)
        {
            case NetworkKeys.WaitingPlayerReadyEvent:
                OnReceivePlayerState(t);
                break;
            case NetworkKeys.WaitingInitialSyncEvent:
                OnReceiveInitialInfo(t);
                break;
        }
    }

    public void LoadMap()
    {
        CommonUtil.LoadLevel((string)PhotonNetwork.CurrentRoom.CustomProperties[NetworkKeys.SceneNameKey]);
    }

    IEnumerator WaitUntilFullyJoin()
    {
        //检查房间是否已经开始
        while (!PhotonNetwork.InRoom)
        {
            yield return null;

        }

        // 检查房间是否有密码，以及玩家是否已输入
        if ((string)NetworkWrapper.CurrentRoom.CustomProperties[NetworkKeys.RoomPassword] != string.Empty && !MatchLobby.Instance.JoinedWithPassword)
        {
            // 如果没有，说明玩家是通过匹配进入带密码的房间的，
            // 匹配由 Photon 控制，选房时不会先检查房间有没有密码
            // 所以只能进房间后才能判断
            PhotonNetwork.LeaveRoom();
            Debug.Log("Leaving room because this is a private room.");
            yield break;
        }

        //如果本机是主客户端，说明房间还没初始化
        if (NetworkWrapper.IsMasterClient)
        {
            //那就把状态设为等待
            SetWaitingState(WaitingState.Waiting);
        }
        else//if is a normal client
        {
            //检查等待房间的状态
            if ((WaitingState)PhotonNetwork.CurrentRoom.CustomProperties[NetworkKeys.WaitingState] == WaitingState.Started)//if is started
            {
                //直接进入游戏地图，跳过等待房间菜单
                PhotonNetwork.IsMessageQueueRunning = false;
                LoadMap();
                yield break;
            }
        }

        //如果还没开始 -> 显示 UI 并隐藏 Lobby 菜单
        LobbyUI.Instance.HideAll();
        OnLocalEnterInWaitingRoom();
        AutoJoinToTeam();
        yield return new WaitForSeconds(0.5f);
        WaitingRoomUIBase.Instance.SetActive(true);
        LobbyUI.Instance.blackScreenFader.FadeOut(0.4f);
    }

    IEnumerator MoveToMap()
    {
        WaitingRoomUIBase.Instance.ShowLoadingScreen(true);
        yield return new WaitForSeconds(2);
        yield return LobbyUI.Instance.blackScreenFader.FadeIn(0.3f);
        LoadMap();
    }

    public override bool IsPlayerReady(Player player)
    {
        return readyPlayers.Contains(player.ActorNumber);
    }

    public override bool IsLocalReady()
    {
        return isLocalReady;
    }

    #region Photon Callbacks
    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnJoinedRoom()
    {
        if (GameCore.GameData.UsingWaitingRoom())
        {
            StartCoroutine(WaitUntilFullyJoin());
        }
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnLeftRoom()
    {
        WaitingRoomUIBase.Instance.SetActive(false);
        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonNetwork.InLobby) { PhotonNetwork.JoinLobby(); }
            else { Debug.Log("Leave Pre Match but already connected to lobby."); LobbyUI.Instance.blackScreenFader.FadeOut(0.5f); }
        }
        else
        {
            Debug.Log("Disconnected from Pre-Match");
        }
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        WaitingRoomUIBase.Instance.UpdatePlayerCount();
        if (NetworkWrapper.IsMasterClient)
        {
            HashTable table = new HashTable();
            string rp = string.Join(",", readyPlayers.Select(x => x.ToString()).ToArray());
            table.Add(NetworkKeys.PlayerID, rp);

            PhotonNetwork.RaiseEvent(NetworkKeys.WaitingPlayerReadyEvent, table, new RaiseEventOptions() { TargetActors = new int[] { newPlayer.ActorNumber } }, SendOptions.SendReliable);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        WaitingRoomUIBase.Instance.InstancePlayerList();
        if (readyPlayers.Contains(otherPlayer.ActorNumber)) { readyPlayers.Remove(otherPlayer.ActorNumber); }
    }

    public void OnRoomPropertiesUpdate(HashTable propertiesThatChanged)
    {
        //如果等待状态发生变化
        if (propertiesThatChanged.ContainsKey(NetworkKeys.WaitingState))
        {
            WaitingState state = (WaitingState)propertiesThatChanged[NetworkKeys.WaitingState];
            if (state == WaitingState.Started)//if the master client start the game
            {
                PhotonNetwork.IsMessageQueueRunning = false;
                //加载地图场景
                StartCoroutine(MoveToMap());
            }
        }
        else
        {
            WaitingRoomUIBase.Instance.UpdateRoomInfoUI();
            if (propertiesThatChanged.ContainsKey(NetworkKeys.GameModeKey) || propertiesThatChanged.ContainsKey(NetworkKeys.TeamKey))
            {
                if (!isOneTeamModeUpdate && NetworkWrapper.LocalPlayer.GetPlayerTeam() == Team.All)
                {
                    AutoJoinToTeam();
                }
                WaitingRoomUIBase.Instance.InstancePlayerList();
            }

            if (propertiesThatChanged.ContainsKey(NetworkKeys.PlayerTotalScore) || propertiesThatChanged.ContainsKey(NetworkKeys.UserRole))
            {
                WaitingRoomUIBase.Instance.InstancePlayerList();
            }
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        WaitingRoomUIBase.Instance.UpdatePlayerCount();
        WaitingRoomUIBase.Instance.InstancePlayerList();
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        WaitingRoomUIBase.Instance.UpdateRoomInfoUI();
    }
    #endregion

    [System.Serializable]
    public enum PlayerState
    {
        NoReady = 0,
        Ready = 1,
    }

    [System.Serializable]
    public enum WaitingState
    {
        Waiting = 0,
        Started = 1,
    }
}