using UnityEngine;

public abstract class CameraMotionBase : MonoBehaviourBase
{
    /// <summary>
    /// 给相机运动额外增加一段旋转
    /// </summary>
    /// <param name="rotation"></param>
    public abstract void AddCameraRotation(Quaternion rotation);

    public abstract void SetActiveBreathing(bool active, float amplitude = 0);
}