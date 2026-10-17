public abstract class WeaponSwayBase : MonoBehaviourBase
{

    public abstract void SetMotionMultiplier(float multiplier);

    public abstract void UseAimSettings();

    /// <summary>
    /// 把所有设置重置为初始值
    /// </summary>
    public abstract void ResetSettings();
}