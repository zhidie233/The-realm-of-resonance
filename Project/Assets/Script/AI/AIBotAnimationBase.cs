using UnityEngine;

public abstract class AIBotAnimationBase : MonoBehaviourBase
{

    private Animator _animator;
    public Animator m_Animator
    {
        get
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
            return _animator;
        }
    }
    /// 让机器人变成布娃娃
    public abstract void Ragdolled(Vector3 from, bool isExplosion = false);
    /// 处理机器人受伤
    public abstract void OnGetHit();

    public virtual Transform GetHumanBone(HumanBodyBones bodyBone)
    {
        if (m_Animator == null) return null;

        return m_Animator.GetBoneTransform(bodyBone);
    }
}
