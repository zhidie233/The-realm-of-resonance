using Photon.Realtime;
using UnityEngine;

public abstract class ReadyRoomBase : NetworkHelper
{
    public abstract void StartGame();

    public abstract void JoinToTeam(Team team);

    public abstract void SetLocalPlayerReady();

    public abstract bool IsPlayerReady(Player player);

    public abstract bool IsLocalReady();

    private static ReadyRoomBase _instance;
    public static ReadyRoomBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ReadyRoomBase>();
            }
            return _instance;
        }
    }
}