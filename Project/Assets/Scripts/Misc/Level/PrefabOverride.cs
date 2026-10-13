using UnityEngine;

/// <summary>
/// Use this script to override the default player prefabs set in GameData -> PlayerTeam1 and PlayerTeam2
/// Attach this script in any game object of the map scene where you wanna override the player prefabs
/// </summary>
public class PrefabOverride : MonoBehaviour
{
    public PlayerNetworkHandler Team1Player, Team2Player;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        GameController.Instance.OverridePlayerPrefab = this;
    }

    /// <summary>
    /// 
    /// </summary>
    public GameObject GetPlayerForTeam(Team team)
    {
        var player = Team1Player.gameObject;
        if (team == Team.Team2)
        {
            player = Team2Player.gameObject;
            if (player == null) player = GameConfig.Instance.Player2.gameObject;
        }
        else
        {
            if (player == null) player = GameConfig.Instance.Player1.gameObject;
        }

        return player;
    }
}