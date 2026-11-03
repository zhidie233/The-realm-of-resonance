using System;
using UnityEngine;

[Serializable]
public class HitBoxInfo
{
    public HumanBodyBones Bone;
    [Range(0.5f, 10)] public float DamageMultiplier = 1.0f;
    public Collider collider;
}
