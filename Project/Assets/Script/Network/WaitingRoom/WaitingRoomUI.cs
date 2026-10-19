using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class WaitingRoomUI : WaitingRoomUIBase
{
    [Header("References")]
    public GameObject Content;
    public GameObject WaitingPlayerPrefab;
    public GameObject LoadingMapUI;
    public GameObject StartScreen;
    public GameObject waitingRequiredPlayersUI;
    public RectTransform PlayerListPanel;
    public TextMeshProUGUI RoomNameText;
    public TextMeshProUGUI MapNameText;
    public TextMeshProUGUI GameModeText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI GoalText;
    public TextMeshProUGUI BotsText;
    public TextMeshProUGUI FriendlyFireText;
    public TextMeshProUGUI PlayerCountText;
    public Image MapPreview;
    public List<RectTransform> PlayerListHeaders = new List<RectTransform>();
    public Button[] readyButtons;

    private List<WaitingPlayerUIBase> playerListCache = new List<WaitingPlayerUIBase>();

    private void OnEnable()
    {
        Content.SetActive(false);
    }

    public override void ShowLoadingScreen(bool show)
    {
        LoadingMapUI.SetActive(show);
    }

    public override void SetActive(bool active)
    {
        if (active)
        {
            UpdateRoomInfoUI();
            InstancePlayerList();
            Content.SetActive(true);
            StartScreen.SetActive(true);
        }
        else
        {
            StartScreen.SetActive(false);
            Content.SetActive(false);
            LobbyUI.Instance.blackScreenFader.FadeOut(0.5f);
        }
    }

    public override void InstancePlayerList()
    {
        playerListCache.ForEach(x => { if (x != null) { Destroy(x.gameObject); } });
        playerListCache.Clear();

        Player[] list = PhotonNetwork.PlayerList;
        List<Player> secondTeam = new List<Player>();
        bool otm = isOneTeamModeUpdate;
        PlayerListHeaders.ForEach(x => x.gameObject.SetActive(!otm));
        for (int i = 0; i < list.Length; i++)
        {
            if (otm)
            {
                if (NetworkWrapper.IsMasterClient)
                {
                    if(list[i].GetPlayerTeam() != Team.All)
                    {
                        list[i].SetPlayerTeam(Team.All);
                    }
                }
                SetPlayerToList(list[i]);
            }
            else
            {
                if (list[i].GetPlayerTeam() == Team.Team1)
                {
                    SetPlayerToList(list[i]);   
                }
                else if (list[i].GetPlayerTeam() == Team.Team2)
                {
                    secondTeam.Add(list[i]);
                }
            }
        }
        if (!otm) { PlayerListHeaders[1].SetAsLastSibling(); }
        if (secondTeam.Count > 0)
        {          
            for (int i = 0; i < secondTeam.Count; i++)
            {
                SetPlayerToList(secondTeam[i]);
            }
        }
        UpdatePlayerCount();
    }

    public void SetPlayerToList(Player player)
    {
        GameObject g = Instantiate(WaitingPlayerPrefab) as GameObject;
        var wp = g.GetComponent<WaitingPlayerUIBase>();
        wp.SetInfo(player);
        g.transform.SetParent(PlayerListPanel, false);
        playerListCache.Add(wp);
    }
   
    public override void UpdateRoomInfoUI()
    {
        GameMode mode = GetGameModeUpdated;
        Room room = PhotonNetwork.CurrentRoom;
        RoomNameText.text = room.Name.ToUpper();
        string mapName = (string)room.CustomProperties[NetworkKeys.SceneNameKey];    
        var si = GameConfig.Instance.AllScenes.Find(x => x.RealSceneName == mapName);
        MapPreview.sprite = si.Preview;
        MapNameText.text = si.ShowName.ToUpper();
        GameModeText.text = mode.GetName().ToUpper();
        int t = (int)room.CustomProperties[NetworkKeys.TimeRoomKey];
        TimeText.text = (t / 60).ToString().ToUpper() + ":00";
        BotsText.text = string.Format("BOTS: {0}", (bool)room.CustomProperties[NetworkKeys.WithBotsKey] ? "ON" : "OFF");
        FriendlyFireText.text = string.Format("友方伤害: {0}", (bool)room.CustomProperties[NetworkKeys.RoomFriendlyFire] ? "ON" : "OFF");       
        UpdatePlayerCount();
        readyButtons[0].gameObject.SetActive(NetworkWrapper.IsMasterClient);
        readyButtons[1].gameObject.SetActive(!NetworkWrapper.IsMasterClient);
        readyButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = WaitingRoomBase.Instance.IsLocalReady() ? "取消".Localized(67).ToUpper() : "准备".Localized(184).ToUpper();

        string goal = room.CustomProperties[NetworkKeys.RoomGoal].ToString();
        if (goal == "0" || string.IsNullOrEmpty(goal))
        {
            GoalText.text = GetGameModeUpdated.GetModeInfo().GoalName.ToUpper();
        }
        else
        {
            GoalText.text = $"{goal} {GetGameModeUpdated.GetModeInfo().GoalName.ToUpper()}";
        }
    }


    public override void UpdatePlayerCount()
    {
        int required = GetGameModeUpdated.GetGameModeInfo().RequiredPlayersToStart;
        if(required > 1)
        {
            bool allRequired = (PhotonNetwork.PlayerList.Length >= required);
            readyButtons[0].interactable = (NetworkWrapper.IsMasterClient && PhotonNetwork.PlayerList.Length >= required);
            PlayerCountText.text = string.Format("{0} OF {2} PLAYERS ({1} MAX)", PhotonNetwork.PlayerList.Length, PhotonNetwork.CurrentRoom.MaxPlayers, required);
            waitingRequiredPlayersUI?.SetActive(!allRequired);
        }
        else
        {
            readyButtons[0].interactable = true;
            waitingRequiredPlayersUI?.SetActive(false);
            PlayerCountText.text = string.Format("{0} PLAYERS ({1} MAX)", PhotonNetwork.PlayerList.Length, PhotonNetwork.CurrentRoom.MaxPlayers);
        }

        int spectatorsCount = GetSpectatorsCount();
        if(spectatorsCount > 0)
        {
            PlayerCountText.text += $" 观战 {spectatorsCount}";
        }
    }

    public override void UpdateAllPlayersStates()
    {
        playerListCache.ForEach(x => { if(x != null) x.UpdateState(); });
    }

    public void SetLocalReady()
    {
        WaitingRoomBase.Instance.SetLocalPlayerReady();
        readyButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = WaitingRoomBase.Instance.IsLocalReady() ? "取消".Localized(67).ToUpper() : "准备".Localized(184).ToUpper();
    }

    public void MasterStartTheGame()
    {
        WaitingRoomBase.Instance.StartGame();
    }

    public void EnterAsSpectator()
    {
        LobbyUI.ShowConfirmationWindow(GameTexts.EnterAsSpectator.Localized(209), () =>
        {
            WaitingRoomBase.Instance.JoinToTeam(Team.None);
            ObserverModeBase.EnterAsSpectator = true;
        });
    }

    private int GetSpectatorsCount()
    {
        int count = 0;
        var players = PhotonNetwork.PlayerList;
        foreach (var player in players)
        {
            if(player.GetPlayerTeam() == Team.None)
            {
                count++;
            }
        }
        return count;
    }

    public void LeaveRoom(bool comfirmed)
    {
        if (comfirmed)
        {
            LobbyUI.Instance.blackScreenFader.FadeIn(0.5f);
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            LobbyUI.ShowConfirmationWindow(GameTexts.LeaveRoomConfirmation.Localized(211), () =>
            {
                LeaveRoom(true);
            });
        }      
    }

  
}