using UnityEngine;

public class SpectatorMode : SpectatorModeBase
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
        
        UtilityHelper.LockCursor(active);
        if (active)
        {
            SetActiveUI(true);
            RoomCameraBase.Instance.SetCameraMode(RoomCameraBase.CameraMode.Spectator);
            RoomCameraBase.Instance.SetActive(active);
            UIReferences.Instance.ShowMenu(false);
            PauseMenuBase.Instance.CloseMenu();
            UIReferences.Instance.SetActiveChangeTeamButton(false);
        }
        else
        {
            SetActiveUI(false);
            if (GameConfig.Instance.onLeaveSpectatorMode == LeaveSpectatorModeAction.ReturnToLobby)
            {
                UtilityHelper.LockCursor(false);
                UIReferences.Instance.leaveRoomConfirmation.AskConfirmation(GameTexts.LeaveMatchWarning.Localized("areusulega"), () =>
                {
                    RoomMenu.Instance.LeaveRoom(false);
                }, () => { UtilityHelper.LockCursor(true); });
                return;
            }

            UIReferences.Instance.SetActiveChangeTeamButton(true);
            RoomCameraBase.Instance.ResetCamera();

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