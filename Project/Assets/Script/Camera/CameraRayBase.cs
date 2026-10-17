using System;
using UnityEngine;

public abstract class CameraRayBase : MonoBehaviourBase
{
    public struct DetecableInfo
    {
        public string Name;
        public byte ID;
        public Action<bool> Callback;
    }

    public float ExtraRayDistance { get; set; } = 0;

    /// <summary>
    /// 相机当前是否在检测碰撞体？
    /// </summary>
    public abstract bool IsCurrentlyDetecting { get; set; }

    /// <summary>
    /// 想检测物体是否出现在本地玩家视野前方时
    /// 在这个函数里注册一个回调
    /// </summary>
    public abstract byte AddTrigger(DetecableInfo detecableInfo);

    /// <summary>
    /// 不再需要检测时记得移除触发器。
    /// </summary>
    public abstract void RemoveTrigger(DetecableInfo detecableInfo);

    public abstract byte SetActiver(bool add, byte id);
}