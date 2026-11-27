using UnityEngine;
using Resonance.Core;
namespace Resonance.Combat
{
    /// 所有武器投掷物（子弹、手雷等）的基类
    public abstract class ProjectileBase : NetworkActorBehaviour
    {
        /// 初始化投掷物
        public abstract void InitProjectile(ShotData data);
    }
}
