using System;
using UnityEngine;

[Serializable]
public class GameModeSettings
{

    [Serializable]
    public enum OnRoundStartedSpawn
    {
        SpawnAfterSelectTeam,
        WaitUntilRoundFinish,
    }

    public enum OnPlayerDie
    {
        SpawnAfterDelay,
        SpawnAfterRoundFinish,
    }

    public enum RoundModeAllowedOptions
    {
        AllowBoth,
        RoundModeOnly,
        SingleRoundOnly,
        OneRoundOfMultipleRoundsOnly,
    }

    /// <summary>
    /// 游戏模式名称
    /// 显示给玩家的名称
    /// </summary>
    public string ModeName;

    /// <summary>
    /// 游戏模式内部标识
    /// 此模式的唯一枚举标识
    /// </summary>
    public GameMode gameMode;

    /// <summary>
    /// 此模式是否在游戏中启用？
    /// </summary>
    [GameToggle] public bool isEnabled = true;

    /// <summary>
    /// 此模式是否允许与机器人对战？
    /// </summary>
    [Header("Settings")]
    [GameToggle] public bool supportBots = false;

    /// <summary>
    /// 此模式强制自动分配队伍
    /// </summary>
    [GameToggle] public bool AutoTeamSelection = false;

    /// <summary>
    /// 对局开始后是否将房间从服务器列表隐藏？
    /// 这样新玩家无法中途加入对局
    /// </summary>
    [GameToggle] public bool UnlistGameAfterStarted = false;

    /// <summary>
    /// 对局所需的最少玩家数，
    /// 达到后游戏才能开始
    /// </summary>
    [Range(1, 32)] public int RequiredPlayersToStart = 1;

    /// <summary>
    /// 对局开始后玩家加入时的行为
    /// </summary>
    public OnRoundStartedSpawn onRoundStartedSpawn = OnRoundStartedSpawn.SpawnAfterSelectTeam;

    /// <summary>
    /// 玩家死亡时的行为
    /// </summary>
    public OnPlayerDie onPlayerDie = OnPlayerDie.SpawnAfterDelay;

    /// <summary>
    /// 允许玩家选择游戏模式的回合模式
    /// 或强制指定回合模式
    /// </summary>
    public RoundModeAllowedOptions RoundModeAllowed = RoundModeAllowedOptions.AllowBoth;

    /// <summary>
    /// 此游戏模式的得分/目标名称
    /// </summary>
    public string GoalName = "Kills";

    /// <summary>
    /// 此游戏模式下玩家能否拾取武器？
    /// </summary>
    [GameToggle] public bool allowedPickupWeapons = true;

    /// <summary>
    /// 此游戏模式允许的最大玩家数选项
    /// 这些选项会出现在房间创建窗口中
    /// 最大玩家数是允许的总人数，在双队伍模式中每队
    /// 人数等于最大玩家数的一半
    /// </summary>
    [Header("Options")]
    public int[] maxPlayers = new int[] { 6, 2, 4, 8 };

    /// <summary>
    /// 此模式选项中的最大得分、击杀数等目标值
    /// 这些选项会出现在房间创建窗口中
    /// </summary>
    public int[] GameGoalsOptions = new int[] { 50, 100, 150, 200 };

    /// <summary>
    /// 此游戏模式的回合时间限制选项
    /// 这些选项会出现在房间创建窗口中
    /// 时间以秒为单位，60 = 1 分钟
    /// </summary>
    public int[] timeLimits = new int[] { 900, 600, 1200, 300 };

    public string GetGoalFullName(int goalID) { return string.Format("{0} {1}", GameGoalsOptions[goalID], GoalName); }

    /// <summary>
    /// 获取游戏模式的目标名称
    /// 例如：击杀、占领、得分、积分等
    /// </summary>
    public string GetGoalName(int goalID)
    {
        if (GameGoalsOptions.Length <= 0) return GoalName;
        return $"{GameGoalsOptions[goalID]} {GoalName}";
    }

    /// <summary>
    /// 按 GameGoalsOptions 列表中的索引获取目标值
    /// </summary>
    /// <returns></returns>
    public int GetGoalValue(int goalID)
    {
        if (GameGoalsOptions.Length <= 0) return 0;
        if (goalID >= GameGoalsOptions.Length) return GameGoalsOptions[GameGoalsOptions.Length - 1];

        return GameGoalsOptions[goalID];
    }

    public bool HasForcedRoundMode() => RoundModeAllowed != RoundModeAllowedOptions.AllowBoth;

    public RoundStyle GetAllowedRoundMode()
    {
        if (RoundModeAllowed == RoundModeAllowedOptions.RoundModeOnly) return RoundStyle.Rounds;
        return RoundStyle.OneMacht;
    }
}