using UnityEngine;

/// <summary>
/// 用此脚本覆盖 GameData -> PlayerTeam1 和 PlayerTeam2 中设置的默认玩家预制体
/// 把此脚本挂到需要覆盖玩家预制体的地图场景中任意游戏物体上
/// </summary>
public class PrefabOverride : MonoBehaviour
{
    public PlayerNetworkHandler Team1Player, Team2Player;

    private void Awake()
    {
        GameController.Instance.OverridePlayerPrefab = this;
    }

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