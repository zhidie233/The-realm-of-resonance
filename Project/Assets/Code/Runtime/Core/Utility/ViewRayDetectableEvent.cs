using UnityEngine;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{
    /// 本地玩家看向此对象时，脚本触发 OnDetected 或 UnDetected 事件
    /// 需要碰撞体，且本地玩家的 ViewRay.Checking 必须为 true。
    /// 参见 ViewRayActiveTrigger.cs
    /// 
    /// 工作原理
    /// 
    /// 出于性能考虑，玩家相机不会每帧检测（RayCast）。
    /// 首先要告诉它何时开始检测（发射射线）
    /// 做法是把本地玩家 ViewRay 的 'Checking' 布尔值设为 true；
    /// 可以用一个触发器碰撞体表示检测区域：玩家进入触发器时开始检测，
    /// 离开时停止，本实现见 ViewRayActiveTrigger.cs，
    public class ViewRayDetectableEvent : MonoBehaviour, IRayDetectable
    {
        public UnityEngine.Events.UnityEvent onDetected;
        public UnityEngine.Events.UnityEvent onUndetected;

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
