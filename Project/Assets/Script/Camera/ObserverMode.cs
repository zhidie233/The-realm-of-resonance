using UnityEngine;
using System;

public class ObserverMode : MonoBehaviourBase
{
    [Serializable]
    public enum LeaveSpectatorModeAction
    {
        AllowSelectTeam,
        ReturnToLobby,
    }
    /// 观战模式是否处于激活状态？
    public virtual bool isActive 
    { 
        get; 
        set; 
    } = false;
    /// 本地玩家是否以观战者身份进入了当前对局？
    private static bool s_enterAsSpectator = false;
    public static bool EnterAsSpectator
    {
        get => s_enterAsSpectator;
        set => s_enterAsSpectator = value;
    }
    /// 玩家能否在进入观战模式后选择队伍？
    public virtual bool CanSelectTeam()
    {
        if (!isActive) return true;
        return GameConfig.Instance.onLeaveSpectatorMode == LeaveSpectatorModeAction.AllowSelectTeam;
    }

    private static ObserverMode _instance;
    public static ObserverMode Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ObserverMode>();
            }
            return _instance;
        }
    }

    [GameToggle] public bool leaveSpectatorWithEscape = false;
    [SerializeField] private GameObject content = null;

    protected override void Awake()
    {
        base.Awake();
        if (EnterAsSpectator)
        {
            EnterAsSpectator = false;
            GameController.Instance.FirstSpawnDone = true;
            SetActiveSpectatorMode(true);
        }
        else
        {
            SetActiveUI(false);
        }
    }

    public void SetActiveSpectatorMode(bool active)
    {
        
        CommonUtil.LockCursor(active);
        if (active)
        {
            SetActiveUI(true);
            LobbyCamera.Instance.SetCameraMode(LobbyCamera.CameraMode.Spectator);
            LobbyCamera.Instance.SetActive(active);
            UIReferences.Instance.ShowMenu(false);
            PauseMenu.Instance.CloseMenu();
            UIReferences.Instance.SetActiveChangeTeamButton(false);
        }
        else
        {
            SetActiveUI(false);
            if (GameConfig.Instance.onLeaveSpectatorMode == LeaveSpectatorModeAction.ReturnToLobby)
            {
                CommonUtil.LockCursor(false);
                UIReferences.Instance.leaveRoomConfirmation.AskConfirmation(GameTexts.LeaveMatchWarning, () =>
                {
                    LobbyMenu.Instance.LeaveRoom(false);
                }, () => { CommonUtil.LockCursor(true); });
                return;
            }

            UIReferences.Instance.SetActiveChangeTeamButton(true);
            LobbyCamera.Instance.ResetCamera();

            if (!GameController.Joined)
            {
                UIReferences.Instance.ShowMenu(true);
                UIReferences.Instance.SetUpJoinButtons(true);
            }
            PauseMenu.Instance.OpenMenu();
        }
        isActive = active;
    }

    public override void OnUpdate()
    {
        ListenInput();
    }

    void ListenInput()
    {
        if (!leaveSpectatorWithEscape) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetActiveSpectatorMode(false);
        }
    }

    public void SetActiveUI(bool active) => content.SetActive(active);
}