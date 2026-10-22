using UnityEngine;

public abstract class PlayerRagdollBase : MonoBehaviour
{
    public struct RagdollInfo
    {
        public Vector3 ForcePosition;
        public Vector3 Velocity;
        public bool IsFromExplosion;
        public bool AutoDestroy;
        public Transform RightHandChild;
    }
    /// 把远程玩家变成布娃娃
    public abstract void Ragdolled(RagdollInfo info);
    /// 把本地玩家变成布娃娃
    public abstract void SetLocalRagdoll(RagdollInfo info);
    /// 启用/停用布娃娃的所有碰撞体和刚体
    public abstract void SetActiveRagdollPhysics(bool active);
    /// 让该布娃娃碰撞体忽略或检测指定的碰撞体
    public abstract void IgnoreColliders(Collider[] list, bool ignore);
    /// 在这个函数里应让该玩家碰撞体忽略本地玩家的碰撞体。
    public abstract void IgnorePlayerCollider();
}
