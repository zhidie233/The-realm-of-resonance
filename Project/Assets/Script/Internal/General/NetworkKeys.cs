using UnityEngine;

public static class NetworkKeys
{
    // 房间哈希表键
    public const string TimeRoomKey = "rr.time";
    public const string GameModeKey = "rr.mode";
    public const string SceneNameKey = "rr.scene";
    public const string CustomSceneName = "rr.customScene";
    public const string RoomRoundKey = "rr.round";
    public const string RoomGoal = "rr.goal";
    public const string TeamSelectionKey = "rr.teamSelection";
    public const string Team1Score = "rr.team1Score";
    public const string Team2Score = "rr.team2Score";
    public const string MaxPing = "rr.maxPing";
    public const string RoomPassword = "rr.password";
    public const string WithBotsKey = "rr.withBots";
    public const string TimeState = "rr.timeState";
    public const string ResonanceValueKey = "rr.resonance";
    // 房间内玩家属性键
    public const string TeamKey = "rr.team";
    public const string KillsKey = "rr.kills";
    public const string DeathsKey = "rr.deaths";
    public const string ScoreKey = "rr.score";
    public const string KickKey = "rr.kick";
    public const string UserRole = "rr.userRole";
    public const string PlayerTotalScore = "rr.totalScore";
    public const string PlayerID = "rr.playerId";
    public const string WaitingState = "rr.waitingState";
    // 自定义网络事件码
    public const byte KickPlayerEvent = 151;
    public const byte ReadyPlayerReadyEvent = 152;
    public const byte WaitingInitialSyncEvent = 153;
    public const byte WaitingStartGame = 154;
    public const byte ChatEvent = 155;
    public const byte KillFeedEvent = 156;
    public const byte NetworkItemInstance = 157;
    public const byte NetworkItemChange = 158;
    public const byte WeaponPickUpEvent = 159;
    public const byte VoteEvent = 160;
    public const byte CaptureOfFlagMode = 161;
    public const byte DropManager = 162;
    public const byte DoorSystem = 163;
    public const byte CountdownEvent = 164;
    /// 返回每局唯一的键。
    /// <param name="key">键的端点</param>
    public static string GetUniqueKey(string key)
    {
        return string.Format("{0}.{1}.{2}", Application.companyName, Application.productName, key);
    }
    /// 返回指定玩家每局唯一的键。
    /// <param name="key">键的端点</param>
    /// <param name="player">玩家昵称</param>
    public static string GetUniqueKeyForPlayer(string key, string player)
    {
        return string.Format("{0}.{1}.{2}.{3}", Application.companyName, Application.productName, player, key);
    }

    public static bool GetBoolPrefs(this string key, bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1 ? true : false;
    }

    public static void SetBoolPrefs(this string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}
