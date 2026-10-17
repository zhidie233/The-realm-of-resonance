using Photon.Realtime;
using UnityEngine;

public abstract class WaitingRoomBase : PhotonHelper
{
    public abstract void StartGame();

    public abstract void JoinToTeam(Team team);

    public abstract void SetLocalPlayerReady();

    public abstract bool IsPlayerReady(Player player);

    public abstract bool IsLocalReady();

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