using UnityEngine;

public class LauncherFirearm : MonoBehaviour
{
    public float explosionRadius = 10;
    private FirearmController FPWeapon;
    private NetworkFirearm TPWeapon;

    /// 启动时该武器在玩家配装中，由 FPWeapon 调用
    public void Initialize(FirearmController gun)
    {
        FPWeapon = gun;
    }

    /// 按住开火按钮时调用
    public void OnFire()
    {
      
    }

    /// 点击一次开火按钮时调用
    public void OnFireDown()
    {
        if (!FPWeapon.FireRatePassed) return;

        FPWeapon.nextFireTime = Time.time;
        Shoot();
    }

    /// 需要生成投掷物时由 TPWeapon（网络枪）调用
    /// 在这里实现自定义开火逻辑
    public void TPFire(NetworkFirearm networkGun, ExitGames.Client.Photon.Hashtable data)
    {
        TPWeapon = networkGun;
        if(FPWeapon == null) { FPWeapon = TPWeapon.LocalGun; }
        GameObject projectile = null;
        Vector3 instancePosition = (Vector3)data["position"];
        Quaternion instanceRotation = (Quaternion)data["rotation"];

        if (FPWeapon.bulletInstanceMethod == FirearmController.ShotInstanceMethod.Pooled)
        {
            projectile = ObjectRecycler.Instance.Instantiate(FPWeapon.BulletName, instancePosition, instanceRotation);
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

    void Shoot()
    {
        GameObject projectile = null;
        Vector3 position = FPWeapon.muzzlePoint.position;
        Quaternion rotation = FPWeapon.PlayerCamera.transform.rotation;
        if (FPWeapon.bulletInstanceMethod == FirearmController.ShotInstanceMethod.Pooled)
        {
            projectile = ObjectRecycler.Instance.Instantiate(FPWeapon.BulletName, position, rotation);
        }
        else
        {
            projectile = Instantiate(FPWeapon.bulletPrefab, position, rotation) as GameObject;
        }

        var glp = projectile.GetComponent<ProjectileBase>();
        FPWeapon.BuildBulletData();
        FPWeapon.BulletSettings.ImpactForce = explosionRadius;
        glp.InitProjectile(FPWeapon.BulletSettings);
        FPWeapon.PlayFireAudio();
        FPWeapon.WeaponAnimation?.PlayFire(WeaponRigHandler.AnimationFlags.Aiming);
        if (FPWeapon.muzzleFlash != null) { FPWeapon.muzzleFlash.Play(); }
        FPWeapon.bulletsLeft--;
        FPWeapon.Kick();
        FPWeapon.UpdateUI();
        FPWeapon.CheckBullets(1);

        var netData = CommonUtil.CreatePhotonHashTable();
        netData.Add("position", position);
        netData.Add("rotation", rotation);

        FPWeapon.PlayerNetwork.SyncCustomProjectile(netData);
        GameEvents.DispatchLocalPlayerFire(FPWeapon.WeaponID);
    }
}