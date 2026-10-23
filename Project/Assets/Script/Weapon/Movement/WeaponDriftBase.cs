public abstract class WeaponDriftBase : MonoBehaviourBase
{

    public abstract void SetMotionMultiplier(float multiplier);

    public abstract void UseAimSettings();
    /// 把所有设置重置为初始值
    public abstract void ResetSettings();
}
