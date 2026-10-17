using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class ThrowKits : MonoBehaviourBase
{
    [Header("SETTINGS")]
    public int AmountOfKits = 3;
    /// <summary>
    /// 实例化预制体时强制应用
    /// </summary>
    public float ForceImpulse = 500;
    [Range(1, 4)] public float CallDelay = 1.4f;
    public float dropInstanceDistance = 1f;

    [Header("REFERENCES")]
    /// <summary>
    /// 用于实例化的预制体
    /// </summary>
    public GameObject DropCallerPrefab;
    public AudioClip SpawnSound;
    public PlayerClass CurrentPlayerClass { get; set; } = PlayerClass.Assault;


#if !ACTK_IS_HERE
    private int remaingKits;
#else
    private ObscuredInt remaingKits;
#endif

    void Start()
    {
        CurrentPlayerClass = PlayerClass.Assault.GetSavePlayerClass();
        remaingKits = AmountOfKits;
    }


    public override void OnUpdate()
    {
        if (GameConfig.Instance.isChating) return;
       

        if (GameInput.ThrowKit())
        {
            DispatchThrow();
        }
    }

    public void DispatchThrow()
    {
        if (remaingKits <= 0 || DropCallerPrefab == null) return;

        int id = 0;
        if ((CurrentPlayerClass == PlayerClass.Assault || CurrentPlayerClass == PlayerClass.Recon))
        {
            id = 1;
        }


        ThrowCaller(id);
    }

    void ThrowCaller(int id)
    {
        remaingKits--;
        Vector3 point = (transform.position + (PlayerReferences.PlayerCameraTransform.forward * dropInstanceDistance)) + transform.up;
        GameObject kit = Instantiate(DropCallerPrefab, point, Quaternion.identity) as GameObject;
        kit.GetComponent<DropCallerBase>().SetUp(new DropCallerBase.DropData()
        {
            KitID = id,
            Delay = CallDelay
        });
        kit.GetComponent<Rigidbody>().AddForce(GameCore.LocalPlayerReferences.PlayerCameraTransform.forward * ForceImpulse);
        if (SpawnSound)
        {
            AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
        }
    }

    public void AddKits(int amount)
    {
        remaingKits += amount;
    }

    private PlayerReferencesHandler _playerReferences;
    public PlayerReferencesHandler PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.GetComponent<PlayerReferencesHandler>();
            return _playerReferences;
        }
    }
}