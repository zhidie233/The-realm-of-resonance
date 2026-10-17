using UnityEngine;

public abstract class AIAnimationBase : MonoBehaviourBase
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

    /// <summary>
    /// 让机器人变成布娃娃
    /// </summary>
    /// <param name="from"></param>
    /// <param name="isExplosion"></param>
    public abstract void Ragdolled(Vector3 from, bool isExplosion = false);

    /// <summary>
    /// 处理机器人受伤
    /// </summary>
    public abstract void OnGetHit();

    public virtual Transform GetHumanBone(HumanBodyBones bodyBone)
    {
        if (m_Animator == null) return null;

        return m_Animator.GetBoneTransform(bodyBone);
    }
}