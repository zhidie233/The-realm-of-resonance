using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;

public class NetworkStatistics : NetworkHelper, IConnectionCallbacks, ILobbyCallbacks

{
    [Header("Settings")]
    [Range(1,5)]public float RandomTime = 2;
    [Range(1,5)]public float UpdateEach = 10;
	[Header("References")]
    [SerializeField]private GameObject RootUI = null;
    [SerializeField]private TextMeshProUGUI AllRoomText = null;
    [SerializeField]private TextMeshProUGUI AllPlayerText = null;
    [SerializeField]private TextMeshProUGUI AllPlayerInRoomText = null;
    [SerializeField]private TextMeshProUGUI AllPlayerInLobbyText = null;
    [SerializeField]private TextMeshProUGUI PingText = null;
    [SerializeField]private Image PingImage = null;

    private float GetTime;
    private int AllRooms;
    private int AllPlayers;
    private int AllPlayerInRoom;
    private int AllPlayerInLobby;
    private bool Started = false;

    private void Awake()
    {
        if (!Started)
        {
            RootUI.SetActive(PhotonNetwork.IsConnected);
            Started = true;
        }
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnEnable()
    {
        Refresh();
        InvokeRepeating("UpdateRepeting", 0, UpdateEach);
    }

    void UpdateRepeting()
    {
        Refresh();
    }

    void OnDisable()
    {
        CancelInvoke();
        StopAllCoroutines();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void Refresh()
    {
        StopAllCoroutines();
        StartCoroutine(GetStaticsIE());
        GetPing();
    }

    void GetPing()
    {
        int ping = PhotonNetwork.GetPing();
        if (ping <= 150)
        {
            PingImage.color = Color.green;
        }
        else if (ping > 150 && ping < 225)
        {
            PingImage.color = Color.yellow;
        }
        else if (ping > 225)
        {
            PingImage.color = Color.red;
        }
        float percet = ping * 100 / 500;
        PingImage.fillAmount = 1 - (percet * 0.01f);
        PingText.text = ping.ToString();
    }

    IEnumerator GetStaticsIE()
    {
        GetTime = RandomTime;
        while(GetTime > 0)
        {
            GetTime -= Time.deltaTime;
            AllRooms = Random.Range(0, 9999);
            AllPlayers = Random.Range(0, 9999);
            AllPlayerInRoom = Random.Range(0, 9999);
            AllPlayerInLobby = Random.Range(0, 9999);
            Set();
            yield return new WaitForEndOfFrame();
        }
        GetPhotonStatics();
        Set();
    }

    void GetPhotonStatics()
    {
        AllRooms = PhotonNetwork.CountOfRooms;
        AllPlayers = PhotonNetwork.CountOfPlayers;
        AllPlayerInRoom = PhotonNetwork.CountOfPlayersInRooms;
        AllPlayerInLobby = PhotonNetwork.CountOfPlayersOnMaster;
    }

    void Set()
    {
        AllRoomText.text = string.Format(GameTexts.RoomsCreated, AllRooms);
        AllPlayerText.text = string.Format(GameTexts.PlayersOnline, AllPlayers);
        AllPlayerInRoomText.text = string.Format(GameTexts.PlayersPlaying, AllPlayerInRoom);
        AllPlayerInLobbyText.text = string.Format(GameTexts.PlayersInLobby, AllPlayerInLobby);
    }

    public void OnConnected()
    {
        RootUI.SetActive(true);
    }

    public void OnConnectedToMaster()
    {
        RootUI.SetActive(true);
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        RootUI.SetActive(false);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
      
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
     
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
       
    }

    public void OnJoinedLobby()
    {

    }

    public void OnLeftLobby()
    {

    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {

    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        Refresh();
    }
}