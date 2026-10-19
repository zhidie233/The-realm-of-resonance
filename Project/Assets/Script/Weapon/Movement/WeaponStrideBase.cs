using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponStrideBase : MonoBehaviourBase
{
    public float Intensitity { get; set; } = 1;

    /// <summary>
    /// 停止行走摇晃运动
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// 在别处更新行走动画
    /// useAnimation == false -> 由武器摇晃脚本控制运动
    /// else -> 由传入的 'callback' 委托控制，不更新武器摇晃脚本。
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="useAnimation"></param>
    public abstract void AnimatedThis(Action<PlayerState> callback, bool useAnimation);
}