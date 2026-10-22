using UnityEngine;

public static class NetworkKeys
{
    // 房间哈希表键
    public const string TimeRoomKey = "rTr";
    public const string GameModeKey = "rGm";
    public const string SceneNameKey = "rLn";
    public const string CustomSceneName = "rCsn";
    public const string RoomRoundKey = "rGpr";
    public const string RoomGoal = "rG";
    public const string TeamSelectionKey = "rAt";
    public const string RoomFriendlyFire = "rFf";
    public const string Team1Score = "rTs1";
    public const string Team2Score = "rTs2";
    public const string MaxPing = "rMp";
    public const string RoomPassword = "rPsw";
    public const string WithBotsKey = "rWb";
    public const string TimeState = "rTs";
    // 房间内玩家属性键
    public const string TeamKey = "Team";
    public const string KillsKey = "Kills";
    public const string DeathsKey = "Deaths";
    public const string ScoreKey = "Score";
    public const string KickKey = "Kick";
    public const string UserRole = "UserRole";
    public const string PlayerTotalScore = "TotalScore";
    public const string PlayerID = "playerID";
    public const string WaitingState = "WaitingState";
    // 自定义网络事件码
    public const byte KickPlayerEvent = 101;
    public const byte ReadyPlayerReadyEvent = 102;
    public const byte WaitingInitialSyncEvent = 103;
    public const byte WaitingStartGame = 104;
    public const byte ChatEvent = 105;
    public const byte KillFeedEvent = 106;
    public const byte NetworkItemInstance = 111;
    public const byte NetworkItemChange = 112;
    public const byte WeaponPickUpEvent = 113;
    public const byte VoteEvent = 114;
    public const byte CaptureOfFlagMode = 117;
    public const byte DropManager = 118;
    public const byte DoorSystem = 119;
    public const byte CountdownEvent = 120;
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
