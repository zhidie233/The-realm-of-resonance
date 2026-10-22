using System;
using UnityEngine;

public abstract class HitBoxBase : MonoBehaviour, IGameDamageable
{
    public HitBoxInfo hitBoxInfo;
    public HitBoxManager hitBoxManager;
    /// 用它在本地接收伤害并同步给其他客户端
    public abstract void ReceiveDamage(DamageInfo damageData);
}

[Serializable]
public class HitBoxInfo
{
    public HumanBodyBones Bone;
    [Range(0.5f, 10)] public float DamageMultiplier = 1.0f;
    public Collider collider;
}
