using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ResonanceFacility : MonoBehaviour, IGameDamageable
{
    [Header("设施配置")]
    public string resonanceKey;
    
    public virtual void ReceiveDamage(DamageInfo damageData)
    {
        
    }
}

