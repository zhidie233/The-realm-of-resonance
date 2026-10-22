using UnityEngine;

public abstract class CameraImpulseBase : MonoBehaviour
{
    /// 添加或开始一个抖动
    public abstract void SetShake(ShakePreset present, string key = "global");
    /// 移除/停止指定的抖动
    public abstract void RemoveShake(string key);
    /// 停止所有抖动
    public abstract void Stop();
    /// 把相机当前位置/旋转设为原点位置
    public abstract void SetCurrentAsOriginPosition();
}
