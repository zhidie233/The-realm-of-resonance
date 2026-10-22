using UnityEngine;

public class ObserverMode : ObserverModeBase
{
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

    public override void SetActiveSpectatorMode(bool active)
    {
        
        CommonUtil.LockCursor(active);
        if (active)
        {
            SetActiveUI(true);
            LobbyCameraBase.Instance.SetCameraMode(LobbyCameraBase.CameraMode.Spectator);
            LobbyCameraBase.Instance.SetActive(active);
            UIReferences.Instance.ShowMenu(false);
            PauseMenuBase.Instance.CloseMenu();
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
            LobbyCameraBase.Instance.ResetCamera();

            if (!GameController.Joined)
            {
                UIReferences.Instance.ShowMenu(true);
                UIReferences.Instance.SetUpJoinButtons(true);
            }
            PauseMenuBase.Instance.OpenMenu();
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

    public override void SetActiveUI(bool active) => content.SetActive(active);
}