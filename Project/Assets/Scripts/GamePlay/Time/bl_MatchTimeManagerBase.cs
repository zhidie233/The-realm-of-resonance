using System;
using TMPro;

public abstract class bl_MatchTimeManagerBase : bl_MonoBehaviour
{

    public enum RoundFinishCause
    {
        TimeUp,
        GameModeLogic,
        GameFinish,
    }

    // Option to pass when the <see cref="StartNewRound(NewRoundOptions)"/> function is called
    public struct NewRoundOptions
    {
        // The current room data? (Team score, player scores, etc...)
        public bool KeepData { get; set; }

        // Automatically respawn the local player?
        public bool RespawnPlayer { get; set; }

        // <param name="keepData"></param>
        public NewRoundOptions(bool keepData)
        {
            KeepData = keepData;
            RespawnPlayer = true;
        }
    }

    public abstract RoomTimeState TimeState { get; set; }

    // Current time in seconds
    public abstract float CurrentTime { get; set; }

    public abstract int RoundDuration { get; set; }

    public abstract bool IsInitialized { get; set; }

    // Initialize the time manager
    public abstract void Init();

    // Initialize the time manager after been waiting for the players.
    public abstract void InitAfterWaiting();

    // Initialize the time manager after a count down
    public abstract void InitAfterCountdown();

    // Active/Disable the script not the game object You can use for example when you want to handle the match time in a different script
    // <param name="active"></param>
    public abstract void SetActive(bool active);

    // Change the time state
    // <param name="state"></param>
    // Replicate the state on all clients
    public abstract void SetTimeState(RoomTimeState state, bool syncOnAllClients = false);

    // Use to use an custom Finish Round function instead of the default generic one
    // <param name="finishRoundHandler"></param>
    public abstract void SetFinishRoundHandler(Action<RoundFinishCause> finishRoundHandler);

    // Finish the current round if the room is one round only this will finish the match.
    public abstract void FinishRound(RoundFinishCause cause = RoundFinishCause.GameModeLogic);

    // Restart the round time
    public abstract void RestartTime();

    // Pause the time
    // <param name="doPause"></param>
    public abstract void Pause(bool doPause);

    // Restart the time and respawn the player
    // Reset the player properties or keep them?
    public abstract void StartNewRound(NewRoundOptions options);

    // Override the default text UI
    // <param name="timeText"></param>
    public abstract void SetTimeTextRender(TextMeshProUGUI timeText, bool disableCurrent = true);

    // <returns></returns>
    public abstract bool IsTimeUp();

    // <returns></returns>
    public static bool HaveTimeStarted() => Instance.TimeState == RoomTimeState.Started;

    private static bl_MatchTimeManagerBase _instance;
    public static bl_MatchTimeManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_MatchTimeManagerBase>(); }
            return _instance;
        }
    }
}