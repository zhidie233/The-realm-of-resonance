using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public abstract class LobbyRoomListBase : MonoBehaviour
{

    public abstract void SetRoomList(List<RoomInfo> rooms);

    /// <summary>
    /// Clear all the rooms (including the cache) in the list.
    /// </summary>
    public abstract void ClearList();

    private static LobbyRoomListBase _instance;
    public static LobbyRoomListBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<LobbyRoomListBase>();
            if (_instance == null && LobbyUI.Instance != null) _instance = LobbyUI.Instance.GetComponentInChildren<LobbyRoomListBase>(true);
            return _instance;
        }
    }
}
