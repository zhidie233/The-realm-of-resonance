using UnityEngine;
using Game.Core.Motion;

public abstract class CameraImpulseBase : MonoBehaviour
{

    /// <summary>
    /// 添加或开始一个抖动
    /// </summary>
    /// <param name="present"></param>
    public abstract void SetShake(ShakerPreset present, string key = "global");

    /// <summary>
    /// 移除/停止指定的抖动
    /// </summary>
    /// <param name="key"></param>
    public abstract void RemoveShake(string key);

    /// <summary>
    /// 停止所有抖动
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// 把相机当前位置/旋转设为原点位置
    /// </summary>
    public abstract void SetCurrentAsOriginPosition();
}