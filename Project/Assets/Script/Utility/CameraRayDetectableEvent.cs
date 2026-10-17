using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// 本地玩家看向此对象时，脚本触发 OnDetected 或 UnDetected 事件
    /// 需要碰撞体，且本地玩家的 CameraRay.Checking 必须为 true。
    /// 参见 CameraRayActiveTrigger.cs
    /// 
    /// 工作原理
    /// 
    /// 出于性能考虑，玩家相机不会每帧检测（RayCast）。
    /// 首先要告诉它何时开始检测（发射射线）
    /// 做法是把本地玩家 CameraRay 的 'Checking' 布尔值设为 true；
    /// 可以用一个触发器碰撞体表示检测区域：玩家进入触发器时开始检测，
    /// 离开时停止，本实现见 CameraRayActiveTrigger.cs，
    /// </summary>
    public class CameraRayDetectableEvent : MonoBehaviour, IRayDetectable
    {
        public GameEvents.UEvent onDetected;
        public GameEvents.UEvent onUndetected;

        public void OnRayDetectedByPlayer()
        {
            onDetected?.Invoke();
        }

        public void OnUnDetectedByPlayer()
        {
            onUndetected?.Invoke();
        }
    }
}