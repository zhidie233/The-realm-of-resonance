using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerRig : PlayerReferencesCommon
{
    public PlayerNetworkHandler playerNetwork;
    public FirstPersonControllerBase firstPersonController;
    public PlayerHealthManagerBase playerHealthManager;
    public PlayerConfig playerSettings;
    public FirearmInventory gunManager;
    public PlayerAnimationsBase playerAnimations;
    public PlayerRagdollBase playerRagdoll;
    public KickbackBase recoil;
    public CameraImpulseBase cameraShaker;
    public ViewRayBase cameraRay;
    public SwayStrideBase weaponBob;
    public WeaponDriftBase weaponSway;
    public HitBoxManager hitBoxManager;
    public CameraMotionRigBase cameraMotion;
    public CharacterController characterController;
    public PhotonView photonView;
    public Camera playerCamera;
    public Camera weaponCamera;
    public ParentConstraint leftArmTarget;

    [SerializeField] private Animator _playerAnimator = null;
    public override Animator PlayerAnimator
    {
        get
        {
            if (_playerAnimator == null) _playerAnimator = playerAnimations.Animator;
            return _playerAnimator;
        }
        set => _playerAnimator = value;
    }

    [SerializeField] private Transform _botAimTarget = null;
    public override Transform BotAimTarget
    {
        get
        {
            if (_botAimTarget == null) _botAimTarget = transform;
            return _botAimTarget;
        }
        set => _botAimTarget = value;
    }
    
    [PunRPC]
    void RPC_RequestResonanceChange(string resonanceKey, float damage)
    {
        if (ResonanceManager.Instance == null) return;

        // 只有MasterClient会执行这个方法
        ResonanceManager.Instance.ChangeResonance(resonanceKey, damage);
    }

    private Transform _transform;
    public Transform Transform
    {
        get
        {
            if (_transform == null) _transform = transform;
            return _transform;
        }
    }

    public override Team PlayerTeam { get => playerSettings.PlayerTeam; }

    public Vector3 Position => Transform.position;
    public Vector3 LocalPosition => Transform.localPosition;
    public Quaternion Rotation => Transform.rotation;
    public Quaternion LocalRotation => Transform.localRotation;
    public GameObject LocalPlayerObjects => playerSettings.LocalObjects;
    public GameObject RemotePlayerObjects => playerSettings.RemoteObjects;

    private Transform _playerCameraTransform;
    public Transform PlayerCameraTransform
    {
        get
        {
            if (_playerCameraTransform == null) _playerCameraTransform = playerCamera.transform;
            return _playerCameraTransform;
        }
    }

    public static PlayerRig LocalPlayer
    {
        get
        {
            return GameController.Instance.LocalPlayerReferences;
        }
    }

    private PlayerIKBase _playerIK = null;
    public PlayerIKBase playerIK
    {
        get
        {
            if (playerAnimations == null) return null;
            if (_playerIK == null) _playerIK = playerAnimations.GetComponentInChildren<PlayerIKBase>();
            return _playerIK;
        }
    }

    private float _defaultCameraFOV = -1;
    public float DefaultCameraFOV
    {
        get
        {
            if (_defaultCameraFOV == -1) _defaultCameraFOV = playerCamera.fieldOfView;
            return _defaultCameraFOV;
        }
        set
        {
            _defaultCameraFOV = value;
            playerCamera.fieldOfView = _defaultCameraFOV;
        }
    }

    private Collider[] _allColliders;
    public override Collider[] AllColliders
    {
        get
        {
            if (_allColliders == null || _allColliders.Length <= 0)
            {
                _allColliders = transform.GetComponentsInChildren<Collider>();
            }
            return _allColliders;
        }
    }

    public int ViewID => photonView.ViewID;
    public int ActorNumber => photonView.Owner.ActorNumber;


    public override bool IsDeath()
    {
        return playerHealthManager.IsDeath();
    }

    public override int GetHealth()
    {
        return playerHealthManager.GetHealth();
    }

    public override int GetMaxHealth()
    {
        return playerHealthManager.GetMaxHealth();
    }

    public override void IgnoreColliders(Collider[] list, bool ignore)
    {
        for (int e = 0; e < list.Length; e++)
        {
            for (int i = 0; i < AllColliders.Length; i++)
            {
                if (AllColliders[i] != null)
                {
                    Physics.IgnoreCollision(AllColliders[i], list[e], ignore);
                }
            }
        }
    }

    [HideInInspector] public NetworkFirearm EditorSelectedGun = null;
}