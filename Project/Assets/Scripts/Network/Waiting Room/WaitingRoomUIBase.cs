using UnityEngine;

public abstract class WaitingRoomUIBase : PhotonHelper
{

    /// <summary>
    /// 
    /// </summary>
    public abstract void UpdateAllPlayersStates();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public abstract void ShowLoadingScreen(bool show);

    /// <summary>
    /// Show the waiting room menu
    /// </summary>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    public abstract void UpdatePlayerCount();

    /// <summary>
    /// 
    /// </summary>
    public abstract void InstancePlayerList();

    /// <summary>
    /// 
    /// </summary>
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