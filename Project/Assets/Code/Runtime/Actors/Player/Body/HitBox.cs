using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Actors
{

    public class HitBox : MonoBehaviour, IGameDamageable
    {   
        public HitBoxInfo hitBoxInfo;
        public HitBoxManager hitBoxManager;

        /// 用它在本地接收伤害并同步给其他客户端
        public void ReceiveDamage(DamageInfo damageData)
        {
            damageData.Damage = Mathf.FloorToInt(damageData.Damage * hitBoxInfo.DamageMultiplier);
            damageData.isHeadShot = hitBoxInfo.Bone == HumanBodyBones.Head;

            hitBoxManager?.OnHit(damageData, this);
        }
    }
}
