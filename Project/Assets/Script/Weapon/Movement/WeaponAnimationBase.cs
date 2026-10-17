using System;
using UnityEngine;

/// <summary>
/// 处理 FPWeapon 动画所需函数的基类。
/// 自定义武器动画脚本继承此类。
/// </summary>
public abstract class WeaponAnimationBase : MonoBehaviour
{
    /// <summary>
    /// 播放开火动画
    /// </summary>
    /// <returns>整段动画的耗时。</returns>
    public abstract float PlayFire(AnimationFlags flags = AnimationFlags.None);

    /// <summary>
    /// 播放换弹动画
    /// </summary>
    /// <param name="onFinish">换弹动画播完后调用的回调。</param>
    public abstract void PlayReload(float reloadDuration, int[] data, AnimationFlags flags = AnimationFlags.None, Action onFinish = null);

    /// <summary>
    /// 播放掏枪动画
    /// </summary>
    /// <returns>整段动画的耗时。</returns>
    public abstract float PlayTakeIn();

    /// <summary>
    /// 播放收枪动画
    /// </summary>
    /// <returns>整段动画的耗时。</returns>
    public abstract float PlayTakeOut();

    /// <summary>
    /// 处理动画取消。
    /// </summary>
    /// <param name="animationType"></param>
    public abstract void CancelAnimation(WeaponAnimationType animationType);

    /// <summary>
    /// 应返回播放整个序列所需的动画时间。
    /// </summary>
    /// <returns></returns>
    public abstract float GetAnimationDuration(WeaponAnimationType animationType, float[] data = null);

    [Flags]
    public enum AnimationFlags
    {
        None = 0,
        Aiming = 1,
        QuickFire = 2,
        SplitReload = 4,
    }

    public enum WeaponAnimationType : short
    {
        TakeIn,
        TakeOut,
        Reload,
        Fire,
        AimFire,
    }
}