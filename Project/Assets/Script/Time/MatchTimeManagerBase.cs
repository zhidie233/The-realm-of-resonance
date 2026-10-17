using System;
using TMPro;

public abstract class MatchTimeManagerBase : MonoBehaviourBase
{

    public enum RoundFinishCause
    {
        TimeUp,
        GameModeLogic,
        GameFinish,
    }

    // 调用 <see cref="StartNewRound(NewRoundOptions)"/> 函数时传入的选项
    public struct NewRoundOptions
    {
        // 是否保留当前房间数据？（队伍得分、玩家得分等）
        public bool KeepData { get; set; }

        // 是否自动重生本地玩家？
        public bool RespawnPlayer { get; set; }

        public NewRoundOptions(bool keepData)
        {
            KeepData = keepData;
            RespawnPlayer = true;
        }
    }

    public abstract RoomTimeState TimeState { get; set; }

    // 当前时间，单位秒
    public abstract float CurrentTime { get; set; }

    public abstract int RoundDuration { get; set; }

    public abstract bool IsInitialized { get; set; }

    // 初始化时间管理器
    public abstract void Init();

    // 等待玩家结束后初始化时间管理器。
    public abstract void InitAfterWaiting();

    // 倒计时之后初始化时间管理器
    public abstract void InitAfterCountdown();

    // 启用/停用脚本而非游戏对象；例如想用别的脚本处理对局时间时可用
    // <param name="active"></param>
    public abstract void SetActive(bool active);

    // 切换时间状态
    // <param name="state"></param>
    // 把状态同步到所有客户端
    public abstract void SetTimeState(RoomTimeState state, bool syncOnAllClients = false);

    // 用它改用自定义的结束回合函数，而不是默认的通用实现
    // <param name="finishRoundHandler"></param>
    public abstract void SetFinishRoundHandler(Action<RoundFinishCause> finishRoundHandler);

    // 结束当前回合；如果房间只有一局，则同时结束对局。
    public abstract void FinishRound(RoundFinishCause cause = RoundFinishCause.GameModeLogic);

    // 重启回合计时
    public abstract void RestartTime();

    // 暂停计时
    // <param name="doPause"></param>
    public abstract void Pause(bool doPause);

    // 重启计时并重生玩家
    // 重置玩家属性还是保留？
    public abstract void StartNewRound(NewRoundOptions options);

    // 覆盖默认文本 UI
    // <param name="timeText"></param>
    public abstract void SetTimeTextRender(TextMeshProUGUI timeText, bool disableCurrent = true);

    public abstract bool IsTimeUp();

    public static bool HaveTimeStarted() => Instance.TimeState == RoomTimeState.Started;

    private static MatchTimeManagerBase _instance;
    public static MatchTimeManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<MatchTimeManagerBase>(); }
            return _instance;
        }
    }
}