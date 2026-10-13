using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLauncher : CustomGunBase
{
    public float explosionRadious = 10;
    private GunController FPWeapon;
    private RemoteGun TPWeapon;

    public override void Initialitate(GunController gun)
    {
        FPWeapon = gun;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFire()
    {
      
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFireDown()
    {
        if (!FPWeapon.FireRatePassed) return;

        FPWeapon.nextFireTime = Time.time;
        Shoot();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void TPFire(RemoteGun networkGun, ExitGames.Client.Photon.Hashtable data)
    {
        TPWeapon = networkGun;
        if(FPWeapon == null) { FPWeapon = TPWeapon.LocalGun; }
        GameObject projectile = null;
        Vector3 instancePosition = (Vector3)data["position"];
        Quaternion instanceRotation = (Quaternion)data["rotation"];

        if (FPWeapon.bulletInstanceMethod == GunController.BulletInstanceMethod.Pooled)
        {
            projectile = ObjectPoolingBase.Instance.Instantiate(FPWeapon.BulletName, instancePosition, instanceRotation);
        }
        else
        {
            projectile = Instantiate(FPWeapon.bulletPrefab, instancePosition, instanceRotation) as GameObject;
        }
        var glp = projectile.GetComponent<ProjectileBase>();
        TPWeapon.m_BulletData.Damage = 0;
        TPWeapon.m_BulletData.isNetwork = true;
        TPWeapon.m_BulletData.Speed = TPWeapon.LocalGun.bulletSpeed;
        TPWeapon.m_BulletData.Position = TPWeapon.transform.position;
        glp.InitProjectile(TPWeapon.m_BulletData);
        TPWeapon.PlayLocalFireAudio();
    }

    /// <summary>
    /// 
    /// </summary>
    void Shoot()
    {
        GameObject projectile = null;
        Vector3 position = FPWeapon.muzzlePoint.position;
        Quaternion rotation = FPWeapon.PlayerCamera.transform.rotation;
        if (FPWeapon.bulletInstanceMethod == GunController.BulletInstanceMethod.Pooled)
        {
            projectile = ObjectPoolingBase.Instance.Instantiate(FPWeapon.BulletName, position, rotation);
        }
        else
        {
            projectile = Instantiate(FPWeapon.bulletPrefab, position, rotation) as GameObject;
        }

        var glp = projectile.GetComponent<ProjectileBase>();
        FPWeapon.BuildBulletData();
        FPWeapon.BulletSettings.ImpactForce = explosionRadious;
        glp.InitProjectile(FPWeapon.BulletSettings);
        FPWeapon.PlayFireAudio();
        FPWeapon.WeaponAnimation?.PlayFire(WeaponAnimationBase.AnimationFlags.Aiming);
        if (FPWeapon.muzzleFlash != null) { FPWeapon.muzzleFlash.Play(); }
        FPWeapon.bulletsLeft--;
        FPWeapon.Kick();
        FPWeapon.UpdateUI();
        FPWeapon.CheckBullets(1);

        var netData = UtilityHelper.CreatePhotonHashTable();
        netData.Add("position", position);
        netData.Add("rotation", rotation);

        FPWeapon.PlayerNetwork.SyncCustomProjectile(netData);
        GameEvents.DispatchLocalPlayerFire(FPWeapon.GunID);
    }
}