using UnityEngine;

public abstract class CameraMotionRigBase : MonoBehaviourBase
{
    /// 给相机运动额外增加一段旋转
    public abstract void AddCameraRotation(Quaternion rotation);

    public abstract void SetActiveBreathing(bool active, float amplitude = 0);
}
