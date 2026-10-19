using UnityEngine;

public class WeaponEffects : WeaponEffectsBase
{
    public ParticleSystem[] highFxParticles;
    public ParticleSystem[] lowFxParticles;

    private ParticleSystem[] targetParticles;

    /// <summary>
    /// 播放武器开火特效
    /// </summary>
    public override void PlayFireFX()
    {
        if (targetParticles == null) ActiveDependOfTarget();

        foreach (var item in targetParticles)
        {
            if (item == null) continue;
            item.Play();
        }
    }

    /// <summary>
    /// 根据目标平台使用对应粒子
    /// 高配用于 PC 和主机平台
    /// 低配用于移动平台
    /// </summary>
    public void ActiveDependOfTarget()
    {
        if (CommonUtil.isMobile)
        {
            SetActiveList(highFxParticles, false);
            SetActiveList(lowFxParticles, true);
            targetParticles = lowFxParticles;
        }
        else
        {
            SetActiveList(lowFxParticles, false);
            SetActiveList(highFxParticles, true);
            targetParticles = highFxParticles;
        }
    }

    public void SetActiveList(ParticleSystem[] list, bool active)
    {
        foreach (var item in list)
        {
            if (item == null) continue;
            item.gameObject.SetActive(active);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        CommonUtil.DrawWireArc(transform.position, 0.03f, 360, 10, Quaternion.LookRotation(transform.up));
    }

    [ContextMenu("Try FP Auto Place")]
    void AutoPlaceInFPWeapon()
    {
        var weapon = transform.GetComponentInParent<FirearmController>();
        if (weapon == null) return;

        var firePoint = weapon.muzzlePoint;
        if (firePoint == null) return;

        Vector3 p = firePoint.position + (weapon.transform.forward * 0.05f);
        transform.position = p;
    }
}