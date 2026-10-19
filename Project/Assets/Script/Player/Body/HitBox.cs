using UnityEngine;

public class HitBox : HitBoxBase
{   
    /// <summary>
    /// 用它在本地接收伤害并同步给其他客户端
    /// </summary>
    public override void ReceiveDamage(DamageInfo damageData)
    {
        damageData.Damage = Mathf.FloorToInt(damageData.Damage * hitBoxInfo.DamageMultiplier);
        damageData.isHeadShot = hitBoxInfo.Bone == HumanBodyBones.Head;

        hitBoxManager?.OnHit(damageData, this);
    }
}