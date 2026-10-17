using UnityEngine;

public abstract class WaitingRoomUIBase : PhotonHelper
{

    public abstract void UpdateAllPlayersStates();

    public abstract void ShowLoadingScreen(bool show);

    /// <summary>
    /// Show the waiting room menu
    /// </summary>
    public abstract void SetActive(bool active);

    public abstract void UpdatePlayerCount();

    public abstract void InstancePlayerList();

    public abstract void UpdateRoomInfoUI();

    private static WaitingRoomUIBase _instance;
    public static WaitingRoomUIBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WaitingRoomUIBase>();
            }
            return _instance;
        }
    }
}