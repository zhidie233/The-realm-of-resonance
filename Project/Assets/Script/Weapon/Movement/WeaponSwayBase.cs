public abstract class WeaponSwayBase : MonoBehaviourBase
{

    public abstract void SetMotionMultiplier(float multiplier);

    public abstract void UseAimSettings();

    /// <summary>
    /// Reset all the settings to its original value
    /// </summary>
    public abstract void ResetSettings();
}