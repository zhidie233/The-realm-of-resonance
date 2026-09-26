using GFWK.Audio;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class bl_NetworkGun : MonoBehaviour
{
    [Header("设置")]
    public bl_Gun LocalGun;
    public bool useCustomPlayerAnimations = false;
    public int customUpperAnimationID = 20;
    public string customFireAnimationName = "Fire";

    [Header("引用")]
    public GameObject Bullet;
    public ParticleSystem MuzzleFlash;
    public GameObject DesactiveOnOffAmmo;
    public Transform LeftHandPosition;

    // Automatically setup this weapon when enabled
    public bool AutoSetup { get; set; }

    private AudioSource Source;
    private int WeaponID = -1;
    [System.NonSerialized]
    public BulletData m_BulletData = new BulletData();
    Vector3 bulletPosition = Vector3.zero;
    Quaternion bulletRotation = Quaternion.identity;
    Transform Root;

    void Awake()
    {
        SetupAudio();
        Root = transform.root;
    }

    // Update type each is enable
    void OnEnable()
    {
        if (AutoSetup) SetUpType();
    }

    public void SetUpType()
    {
        PlayerSync?.SetNetworkWeapon(Info.Type, this);
        LocalGun?.customWeapon?.Initialitate(LocalGun);
    }

    // Fire Sync in network player
    public void Fire(Vector3 hitPoint, Vector3 inacuracity)
    {
        if (LocalGun != null)
        {
            if (MuzzleFlash)
            {
                PlayMuzzleflash();
                bulletPosition = MuzzleFlash.transform.position;
            }
            else
            {
                bulletPosition = transform.position;
            }

            bulletRotation = Quaternion.LookRotation(hitPoint - bulletPosition);
            //bullet info is set up in start function
            GameObject newBullet = bl_ObjectPoolingBase.Instance.Instantiate(LocalGun.BulletName, bulletPosition, bulletRotation); // create a bullet
            // set the gun's info into an array to send to the bullet
            m_BulletData.Damage = 0;
            m_BulletData.ImpactForce = 0;
            m_BulletData.Speed = LocalGun.bulletSpeed;
            m_BulletData.Inaccuracity = inacuracity;
            m_BulletData.DropFactor = LocalGun.bulletDropFactor;
            m_BulletData.Position = Root.position;
            m_BulletData.isNetwork = true;

            newBullet.GetComponent<bl_ProjectileBase>().InitProjectile(m_BulletData);
            PlayLocalFireAudio();
        }
    }

    public bool FireCustomLogic(ExitGames.Client.Photon.Hashtable data)
    {
        if (LocalGun != null && LocalGun.customWeapon != null)
        {
            LocalGun.customWeapon.TPFire(this, data);
            return true;
        }
        return false;
    }

    // if grenade
    // <param name="s"></param>
    public void GrenadeFire(float s, Vector3 position, Quaternion rotation, Vector3 direction)
    {
        if (LocalGun != null)
        {
            //bullet info is set up in start function
            GameObject newBullet = Instantiate(Bullet, position, rotation) as GameObject; // create a bullet
            // set the gun's info into an array to send to the bullet
            BulletData bulletData = new BulletData();
            bulletData.SetInaccuracity(s, LocalGun.spreadMinMax.y);
            bulletData.Speed = LocalGun.bulletSpeed;
            bulletData.Position = Root.position;
            bulletData.isNetwork = true;

            var proRigid = newBullet.GetComponent<Rigidbody>();
            if (proRigid != null)
            {
                proRigid.AddForce(direction, ForceMode.Impulse);
            }

            newBullet.GetComponent<bl_ProjectileBase>().InitProjectile(bulletData); //bl_Projectile.cs
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    // When is knife only reply sounds
    public void KnifeFire()
    {
        if (LocalGun != null)
        {
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    // <param name="active"></param>
    public void DesactiveGrenade(bool active, Material mat)
    {
        if (Info.Type != GunType.Grenade)
        {
            Debug.LogError("Gun type is not grenade, can't desactive it: " + Info.Type);
            return;
        }
        //when hide network gun / grenade we use method of change material to a invincible
        //due that if desactive the render cause animation  player broken.
        if (DesactiveOnOffAmmo != null)
        {
            DesactiveOnOffAmmo.SetActive(active);
        }
    }

    public void PlayMuzzleflash()
    {
        if (MuzzleFlash == null) return;

        MuzzleFlash.Play();
    }

    public void PlayLocalFireAudio()
    {
        Source.clip = LocalGun.FireSound;
        Source.spread = Random.Range(1.0f, 1.5f);
        Source.Play();
    }

    private void SetupAudio()
    {
        Source = GetComponent<AudioSource>();
        Source.playOnAwake = false;
        Source.spatialBlend = 1;
        if (Info.Type != GunType.Knife)
        {
            Source.maxDistance = bl_AudioController.Instance.maxWeaponDistance;
            Source.minDistance = bl_AudioController.Instance.maxWeaponDistance * 0.09f;
        }
        else
        {
            Source.maxDistance = 5;
            Source.minDistance = 2;
        }
        Source.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
        Source.spatialize = true;
    }

    // Returns the upper state ID That will be used to identify which upper body animations will be played when this weapon is equipped
    // <returns></returns>
    public int GetUpperStateID()
    {
        if (!useCustomPlayerAnimations || customUpperAnimationID <= 20) return (int)Info.Type;
        return customUpperAnimationID;
    }

    public int GetWeaponID
    {
        get
        {
            if (WeaponID == -1)
            {
                if (LocalGun != null)
                {
                    WeaponID = LocalGun.GunID;
                }
            }
            return WeaponID;
        }
    }

    private bl_GunInfo _info = null;
    public bl_GunInfo Info
    {
        get
        {
            if (LocalGun != null)
            {
                if (_info == null) { _info = bl_GameData.Instance.GetWeapon(GetWeaponID); }
                return _info;
            }
            else
            {
                Debug.LogError("This tpv weapon: " + gameObject.name + " has not been defined!");
                return bl_GameData.Instance.GetWeapon(0);
            }
        }
    }

    private bl_PlayerNetwork _pS;
    private bl_PlayerNetwork PlayerSync
    {
        get
        {
            if (_pS == null) { _pS = transform.root.GetComponent<bl_PlayerNetwork>(); }
            return _pS;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        /*  if (Application.isPlaying)
              return;*/

        if (LeftHandPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(LeftHandPosition.position, 0.02f);
            Gizmos.DrawWireSphere(LeftHandPosition.position, 0.05f);
        }
    }
#endif
}