using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ResonanceFacility : MonoBehaviour, IMFPSDamageable
{
    [Header("设施配置")]
    public string resonanceKey;
    
    public virtual void ReceiveDamage(DamageData damageData)
    {
        
    }
}

