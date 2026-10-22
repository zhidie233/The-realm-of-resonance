using System;
using UnityEngine;
/// 处理 FPWeapon 动画所需函数的基类。
/// 自定义武器动画脚本继承此类。
public abstract class WeaponRigBase : MonoBehaviour
{
    /// 播放开火动画
    /// <returns>整段动画的耗时。</returns>
    public abstract float PlayFire(AnimationFlags flags = AnimationFlags.None);
    /// 播放换弹动画
    /// <param name="onFinish">换弹动画播完后调用的回调。</param>
    public abstract void PlayReload(float reloadDuration, int[] data, AnimationFlags flags = AnimationFlags.None, Action onFinish = null);
    /// 播放掏枪动画
    /// <returns>整段动画的耗时。</returns>
    public abstract float PlayTakeIn();
    /// 播放收枪动画
    /// <returns>整段动画的耗时。</returns>
    public abstract float PlayTakeOut();
    /// 处理动画取消。
    public abstract void CancelAnimation(WeaponRigType animationType);
    /// 应返回播放整个序列所需的动画时间。
    public abstract float GetAnimationDuration(WeaponRigType animationType, float[] data = null);

    [Flags]
    public enum AnimationFlags
    {
        None = 0,
        Aiming = 1,
        QuickFire = 2,
        SplitReload = 4,
    }

    public enum WeaponRigType : short
    {
        TakeIn,
        TakeOut,
        Reload,
        Fire,
        AimFire,
    }
}
