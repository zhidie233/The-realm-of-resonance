using UnityEngine;

public abstract class CameraMotionBase : MonoBehaviourBase
{
    /// <summary>
    /// Add an extra camera rotation to the camera motion
    /// </summary>
    /// <param name="rotation"></param>
    public abstract void AddCameraRotation(Quaternion rotation);

    public abstract void SetActiveBreathing(bool active, float amplitude = 0);
}