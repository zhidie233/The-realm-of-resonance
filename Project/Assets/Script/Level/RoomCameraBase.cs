using UnityEngine;

public abstract class RoomCameraBase : MonoBehaviourBase
{
    public enum CameraMode
    {
        MapPreview,
        Spectator
    }

    /// <summary>
    /// 不要通过此脚本（或其子类）移动或旋转相机
    /// </summary>
    public bool BlockSelfMovement
    {
        get;
        set;
    } = false;

    public abstract CameraMode GetCameraMode();

    public abstract void SetCameraMode(CameraMode cameraMode);

    public abstract void SetActive(bool active);

    public abstract void ResetCamera();

    private Camera _cameraComponent = null;
    public virtual Camera GetCamera()
    {
        if (_cameraComponent == null)
        {
            _cameraComponent = GetComponent<Camera>();
        }
        return _cameraComponent;
    }


    private static RoomCameraBase _instance;
    public static RoomCameraBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RoomCameraBase>();
            }
            return _instance;
        }
    }
}
