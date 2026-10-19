using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// 此脚本在玩家进入/离开触发碰撞体时
    /// 启用/关闭本地玩家的相机射线检测。
    /// 
    /// 工作原理
    /// 
    /// 出于性能考虑，玩家相机不会每帧检测（RayCast）。
    /// 首先要告诉它何时开始检测（发射射线）
    /// 做法是把本地玩家 ViewRay 的 'Checking' 布尔值设为 true；
    /// 可以用一个触发器碰撞体表示检测区域：玩家进入触发器时开始检测，
    /// 离开时停止，本实现见 ViewRayActiveTrigger.cs，
    /// </summary>
    public class ViewRayActiveTrigger : MonoBehaviour
    {

        private void OnTriggerEnter(Collider other)
        {
            if (!other.isLocalPlayerCollider()) return;

            SetDetectionActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.isLocalPlayerCollider()) return;

            SetDetectionActive(false);
        }

        private void SetDetectionActive(bool active)
        {
            if (GameCore.LocalPlayerReferences == null) return;

            GameCore.LocalPlayerReferences.cameraRay.IsCurrentlyDetecting = active;
        }
    }
}