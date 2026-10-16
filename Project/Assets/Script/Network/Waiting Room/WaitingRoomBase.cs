using Photon.Realtime;
using UnityEngine;

public abstract class WaitingRoomBase : PhotonHelper
{
    /// <summary>
    /// 
    /// </summary>
    public abstract void StartGame();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamID"></param>
    public abstract void JoinToTeam(Team team);

    /// <summary>
    /// 
    /// </summary>
    public abstract void SetLocalPlayerReady();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public abstract bool IsPlayerReady(Player player);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract bool IsLocalReady();

    /// <summary>
    /// 
    /// </summary>
    private static WaitingRoomBase _instance;
    public static WaitingRoomBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WaitingRoomBase>();
            }
            return _instance;
        }
    }
}