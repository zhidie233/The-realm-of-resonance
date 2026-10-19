using UnityEngine;

public abstract class WaitingRoomUIBase : NetworkHelper
{

    public abstract void UpdateAllPlayersStates();

    public abstract void ShowLoadingScreen(bool show);

    /// <summary>
    /// 显示等待房间菜单
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