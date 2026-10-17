using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIShooterAttackBase : PhotonHelper
{
    public enum FireReason
    {
        Normal,
        OnMove,
        Forced,
    }

    public abstract bool IsFiring
    {
        get;
        set;
    }

    public abstract void Fire(FireReason fireReason = FireReason.Normal);

    public abstract void OnDeath();

    public abstract Vector3 GetFirePosition();
}