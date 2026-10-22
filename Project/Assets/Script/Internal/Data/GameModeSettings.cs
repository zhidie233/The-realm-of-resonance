using System;
using UnityEngine;

[Serializable]
public class GameModeSettings
{

    [Serializable]
    public enum OnRoundBegin
    {
        SpawnAfterSelectTeam,
        WaitUntilRoundFinish,
    }

    public enum OnDeath
    {
        SpawnAfterDelay,
        SpawnAfterRoundFinish,
    }

    public enum RoundFormatOptions
    {
        AllowBoth,
        RoundModeOnly,
        SingleRoundOnly,
        OneRoundOfMultipleRoundsOnly,
    }
    /// 游戏模式名称
    /// 显示给玩家的名称
    public string ModeName;
    /// 游戏模式内部标识
    /// 此模式的唯一枚举标识
    public GameMode gameMode;
    /// 此模式是否在游戏中启用？
    [GameToggle] public bool isEnabled = true;
    /// 此模式是否允许与机器人对战？
    [Header("Settings")]
    [GameToggle] public bool botsEnabled = false;
    /// 此模式强制自动分配队伍
    [GameToggle] public bool AutoAssignTeams = false;
    /// 对局开始后是否将房间从服务器列表隐藏？
    /// 这样新玩家无法中途加入对局
    [GameToggle] public bool HideRoomAfterStart = false;
    /// 对局所需的最少玩家数，
    /// 达到后游戏才能开始
    [Range(1, 32)] public int MinPlayersToStart = 1;
    /// 对局开始后玩家加入时的行为
    public OnRoundBegin spawnOnRoundBegin = OnRoundBegin.SpawnAfterSelectTeam;
    /// 玩家死亡时的行为
    public OnDeath onDeath = OnDeath.SpawnAfterDelay;
    /// 允许玩家选择游戏模式的回合模式
    /// 或强制指定回合模式
    public RoundFormatOptions RoundFormat = RoundFormatOptions.AllowBoth;
    /// 此游戏模式的得分/目标名称
    public string GoalName = "Kills";
    /// 此游戏模式下玩家能否拾取武器？
    [GameToggle] public bool allowWeaponPickup = true;
    /// 此游戏模式允许的最大玩家数选项
    /// 这些选项会出现在房间创建窗口中
    /// 最大玩家数是允许的总人数，在双队伍模式中每队
    /// 人数等于最大玩家数的一半
    [Header("Options")]
    public int[] maxPlayers = new int[] { 6, 2, 4, 8 };
    /// 此模式选项中的最大得分、击杀数等目标值
    /// 这些选项会出现在房间创建窗口中
    public int[] GoalValueOptions = new int[] { 50, 100, 150, 200 };
    /// 此游戏模式的回合时间限制选项
    /// 这些选项会出现在房间创建窗口中
    /// 时间以秒为单位，60 = 1 分钟
    public int[] roundTimes = new int[] { 900, 600, 1200, 300 };

    public string GetGoalFullName(int goalID) { return string.Format("{0} {1}", GoalValueOptions[goalID], GoalName); }
    /// 获取游戏模式的目标名称
    /// 例如：击杀、占领、得分、积分等
    public string GetGoalName(int goalID)
    {
        if (GoalValueOptions.Length <= 0) return GoalName;
        return $"{GoalValueOptions[goalID]} {GoalName}";
    }
    /// 按 GoalValueOptions 列表中的索引获取目标值
    public int GetGoalValue(int goalID)
    {
        if (GoalValueOptions.Length <= 0) return 0;
        if (goalID >= GoalValueOptions.Length) return GoalValueOptions[GoalValueOptions.Length - 1];

        return GoalValueOptions[goalID];
    }

    public bool HasForcedRoundMode() => RoundFormat != RoundFormatOptions.AllowBoth;

    public RoundStyle GetAllowedRoundMode()
    {
        if (RoundFormat == RoundFormatOptions.RoundModeOnly) return RoundStyle.Rounds;
        return RoundStyle.OneMacht;
    }
}
