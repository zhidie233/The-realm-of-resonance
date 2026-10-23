using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class NetworkFirearm : MonoBehaviour
{
    [Header("设置")]
    public FirearmController LocalGun;
    public bool useCustomPlayerAnimations = false;
    public int customUpperAnimationID = 20;
    public string customFireAnimationName = "Fire";

    [Header("引用")]
    public GameObject Bullet;
    public ParticleSystem MuzzleFlash;
    public GameObject DesactiveOnOffAmmo;
    public Transform LeftHandPosition;

    // 启用时自动设置这把武器
    public bool AutoSetup { get; set; }

    private AudioSource Source;
    private int WeaponID = -1;
    [System.NonSerialized]
    public ShotData m_BulletData = new ShotData();
    Vector3 bulletPosition = Vector3.zero;
    Quaternion bulletRotation = Quaternion.identity;
    Transform Root;

    void Awake()
    {
        SetupAudio();
        Root = transform.root;
    }

    // 每次启用时更新类型
    void OnEnable()
    {
        if (AutoSetup) SetUpType();
    }

    public void SetUpType()
    {
        PlayerSync?.SetNetworkWeapon(Info.Type, this);
        LocalGun?.customWeapon?.Initialitate(LocalGun);
    }

    // 网络玩家中的开火同步
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
            //子弹信息在 start 函数中设置
            GameObject newBullet = ObjectRecyclerBase.Instance.Instantiate(LocalGun.BulletName, bulletPosition, bulletRotation);
            // 把枪械信息写入数组传给子弹
            m_BulletData.Damage = 0;
            m_BulletData.ImpactForce = 0;
            m_BulletData.Speed = LocalGun.bulletSpeed;
            m_BulletData.Inaccuracity = inacuracity;
            m_BulletData.DropFactor = LocalGun.bulletDropFactor;
            m_BulletData.Position = Root.position;
            m_BulletData.isNetwork = true;

            newBullet.GetComponent<ProjectileBase>().InitProjectile(m_BulletData);
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

    // 若是手雷
    public void GrenadeFire(float s, Vector3 position, Quaternion rotation, Vector3 direction)
    {
        if (LocalGun != null)
        {
            //子弹信息在 start 函数中设置
            GameObject newBullet = Instantiate(Bullet, position, rotation) as GameObject;
            // 把枪械信息写入数组传给子弹
            ShotData bulletData = new ShotData();
            bulletData.SetInaccuracity(s, LocalGun.spreadMinMax.y);
            bulletData.Speed = LocalGun.bulletSpeed;
            bulletData.Position = Root.position;
            bulletData.isNetwork = true;

            var proRigid = newBullet.GetComponent<Rigidbody>();
            if (proRigid != null)
            {
                proRigid.AddForce(direction, ForceMode.Impulse);
            }
            newBullet.GetComponent<ProjectileBase>().InitProjectile(bulletData);
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    // 是刀时只回放音效
    public void KnifeFire()
    {
        if (LocalGun != null)
        {
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    public void DesactiveGrenade(bool active, Material mat)
    {
        if (Info.Type != FirearmType.Grenade)
        {
            Debug.LogError("武器类型不是投掷物，无法禁用：" + Info.Type);
            return;
        }
        //隐藏网络枪/手雷时改用换成不可见材质的方式
        //因为直接禁用渲染器会破坏玩家动画。
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
        if (Info.Type != FirearmType.Knife)
        {
            Source.maxDistance = AudioManager.Instance.maxWeaponDistance;
            Source.minDistance = AudioManager.Instance.maxWeaponDistance * 0.09f;
        }
        else
        {
            Source.maxDistance = 5;
            Source.minDistance = 2;
        }
        Source.rolloffMode = AudioManager.Instance.audioRolloffMode;
        Source.spatialize = true;
    }

    // 返回上半身状态 ID，用于确定装备该武器时播放哪些上半身动画
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
                    WeaponID = LocalGun.WeaponID;
                }
            }
            return WeaponID;
        }
    }

    private FirearmData _info = null;
    public FirearmData Info
    {
        get
        {
            if (LocalGun != null)
            {
                if (_info == null) { _info = GameConfig.Instance.GetWeapon(GetWeaponID); }
                return _info;
            }
            else
            {
                Debug.LogError("第三人称武器 " + gameObject.name + " 尚未定义。");
                return GameConfig.Instance.GetWeapon(0);
            }
        }
    }

    private PlayerNetworkHandler _pS;
    private PlayerNetworkHandler PlayerSync
    {
        get
        {
            if (_pS == null) { _pS = transform.root.GetComponent<PlayerNetworkHandler>(); }
            return _pS;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {

        if (LeftHandPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(LeftHandPosition.position, 0.02f);
            Gizmos.DrawWireSphere(LeftHandPosition.position, 0.05f);
        }
    }
#endif
}
