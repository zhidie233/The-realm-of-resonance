using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

[RequireComponent(typeof(PhotonView))]
public class PlayerNetworkHandler : MonoBehaviourBase, IPunObservable
{
    // 非本方玩家的队伍
    public Team RemoteTeam { get; set; }
    // 本地玩家第一人称状态
    public PlayerFPState FPState = PlayerFPState.Idle;
    // 玩家注视的目标
    [FormerlySerializedAs("HeatTarget")]
    public Transform HeadTarget;
    // 平滑插值量
    public float SmoothingDelay = 8f;
    // 所有远程武器列表
    public List<NetworkFirearm> NetworkGuns = new List<NetworkFirearm>();
    [SerializeField]
    PhotonTransformViewPositionModel m_PositionModel = new PhotonTransformViewPositionModel();
    [SerializeField]
    PhotonTransformViewRotationModel m_RotationModel = new PhotonTransformViewRotationModel();
    public Material InvicibleMat;

    private PlayerTagBase DrawName;
    private bool SendInfo = false;
    private bool isWeaponBlocked = false;
    private NetworkFirearm currentGun;
    private Transform _transform;
    private Vector3 networkHeadLookAt = Vector3.zero;// Head Look to
    private bool networkIsGrounded;
    private string RemotePlayerName = string.Empty;
    private Vector3 networkVelocity;
    PhotonTransformViewPositionControl _positionControl;
    PhotonTransformViewRotationControl _rotationControl;
    bool _receivedNetworkUpdate = false;
    private Vector3 headEuler = Vector3.zero;
    private int currentGunID = -1;

    // 此玩家是否为本地玩家的队友
    public bool isFriend { get; set; }

    // 当前第三人称武器 ID
    public int networkGunID { get; set; }

    // 此玩家手中的当前第三人称武器
    public NetworkFirearm CurrenGun { get; set; }

    // 从此玩家客户端侧接收的当前状态
    public PlayerState NetworkBodyState { get; set; }

    protected override void Awake()
    {
        base.Awake();
        NetworkCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;

        _transform = transform;
        _positionControl = new PhotonTransformViewPositionControl(m_PositionModel);
        _rotationControl = new PhotonTransformViewRotationControl(m_RotationModel);
        DrawName = GetComponent<PlayerTagBase>();
        NetworkGuns.ForEach(x =>
        {
            if (x != null) x.gameObject.SetActive(false);
        });
    }

    private void Start()
    {
        InvokeRepeating(nameof(SlowLoop), 0, 1);
    }

    // photon 的序列化方法
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        _positionControl.OnPhotonSerializeView(_transform.localPosition, stream, info);
        _rotationControl.OnPhotonSerializeView(_transform.localRotation, stream, info);

#if UNITY_EDITOR
        if (isMine == false)
        {
            DoDrawEstimatedPositionError();
        }
#endif

        if (stream.IsWriting)
        {
            //这个玩家归我们所有：把数据发给其他人
            stream.SendNext(GetHeadLookPosition());
            stream.SendNext(fpControler.State);
            stream.SendNext(fpControler.isGrounded);
            stream.SendNext((byte)PlayerReferences.gunManager.GetCurrentGunID);//send as byte, max value is 255.
            stream.SendNext(FPState);
            stream.SendNext(fpControler.Velocity);
        }
        else
        {
            //网络玩家，接收数据
            networkHeadLookAt = (Vector3)stream.ReceiveNext();
            NetworkBodyState = (PlayerState)stream.ReceiveNext();
            networkIsGrounded = (bool)stream.ReceiveNext();
            networkGunID = (int)((byte)stream.ReceiveNext());
            FPState = (PlayerFPState)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            _receivedNetworkUpdate = true;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (GameConfig.Instance.DropGunOnDeath && LobbyMenu.Instance != null && !LobbyMenu.Instance.isApplicationQuitting)
        {
            if (NetGunsRoot != null) NetGunsRoot.gameObject.SetActive(false);
        }
        NetworkCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
    }

    public override void OnUpdate()
    {
        if (isConnected == false || !PhotonNetwork.InRoom) return;

        if (photonView != null && !isMine)
        {
            OnRemotePlayer();
        }
        else
        {
            OnLocalPlayer();
        }
    }

    // 玩家为本机玩家时每帧调用
    void OnLocalPlayer()
    {
        //发送本机玩家状态，供远程动画使用
        PlayerReferences.playerAnimations.BodyState = fpControler.State;
        PlayerReferences.playerAnimations.IsGrounded = fpControler.isGrounded;
        PlayerReferences.playerAnimations.Velocity = fpControler.Velocity;
        PlayerReferences.playerAnimations.FPState = FPState;
    }

    // 玩家为远程玩家时每帧调用
    void OnRemotePlayer()
    {
        if (_transform.parent == null)
        {
            UpdatePosition();
            UpdateRotation();
        }

        HeadTarget.LookAt(networkHeadLookAt);

        PlayerReferences.playerAnimations.BodyState = NetworkBodyState;//send the state of player local for remote animation
        PlayerReferences.playerAnimations.IsGrounded = networkIsGrounded;
        PlayerReferences.playerAnimations.Velocity = networkVelocity;
        PlayerReferences.playerAnimations.FPState = FPState;

        if (!isOneTeamMode)
        {
            //判断远程玩家是队友还是敌人
            if (isFriend)
                OnTeammatePlayer();
            else
                OnEnemyPlayer();
        }
        else
        {
            OnEnemyPlayer();
        }

        if (!isWeaponBlocked)
        {
            CurrentTPVGun();
            currentGunID = networkGunID;
        }
    }

    // 该远程玩家是敌人时每帧调用
    void OnEnemyPlayer()
    {
        playerHealthManager.DamageEnabled = true;
        DrawName.SetActive(ObserverModeBase.Instance.isActive);
    }

    // 该远程玩家是队友时每帧调用
    void OnTeammatePlayer()
    {
        playerHealthManager.DamageEnabled = RoomSettings.Instance.CurrentRoomInfo.friendlyFire;
        DrawName.SetActive(true);
        characterController.enabled = false;

        if (!SendInfo)
        {
            SendInfo = true;
            PlayerReferences.playerRagdoll.IgnorePlayerCollider();
        }

    }

    // 控制远程玩家显示哪把第三人称武器
    // 当前武器应该取本机玩家的，还是取网络数据？
    // 即使已装备武器没变也要再核对一次。
    public void CurrentTPVGun(bool local = false, bool force = false)
    {
        if (PlayerReferences.gunManager == null)
            return;
        if (networkGunID == currentGunID && !force) return;

        //获取本机当前武器 ID 并与远程同步
        for (int i = 0; i < NetworkGuns.Count; i++)
        {
            currentGun = NetworkGuns[i];
            if (currentGun == null) continue;

            int currentID = (local) ? PlayerReferences.gunManager.GetCurrentWeapon().GunID : networkGunID;
            if (currentGun.GetWeaponID == currentID)
            {
                currentGun.gameObject.SetActive(true);
                if (!local)
                {
                    CurrenGun = currentGun;
                    CurrenGun.SetUpType();
                }
            }
            else
            {
                if (currentGun != null)
                    currentGun.gameObject.SetActive(false);
            }
        }
    }

    void SlowLoop()
    {
        if (photonView == null || photonView.Owner == null) return;

        RemotePlayerName = photonView.Owner.NickName;
        RemoteTeam = photonView.Owner.GetPlayerTeam();
        gameObject.name = RemotePlayerName;
        if (DrawName != null) { DrawName.SetName(RemotePlayerName); }
        isFriend = (RemoteTeam == NetworkWrapper.LocalPlayer.GetPlayerTeam());
    }

    // 修改该远程玩家当前的 TPWeapon
    public void SetNetworkWeapon(FirearmType weaponType, NetworkFirearm networkGun)
    {
        PlayerReferences.playerAnimations?.SetNetworkGun(weaponType, networkGun);
    }

    // 通知所有其他客户端同步一发子弹
    public void ReplicateFire(FirearmType weaponType, Vector3 hitPosition, Vector3 inacuracity)
    {
        photonView.RPC(nameof(FireSync), RpcTarget.Others, weaponType, hitPosition, inacuracity);
    }

    // 发送射击同步 RPC 的公开方法
    public void IsFireGrenade(float t_spread, Vector3 pos, Quaternion rot, Vector3 direction)
    {
        photonView.RPC(nameof(FireGrenadeRpc), RpcTarget.Others, new object[] { t_spread, pos, rot, direction });
    }

    // 用当前远程武器同步这次射击，发送必要信息，使弹着方向与本机一致
    [PunRPC]
    void FireSync(FirearmType weaponType, Vector3 hitPosition, Vector3 inaccuracity)
    {
        if (CurrenGun == null) return;
        switch (weaponType)
        {
            case FirearmType.Machinegun:
                CurrenGun.Fire(hitPosition, inaccuracity);
                PlayerReferences.playerAnimations.PlayFireAnimation(FirearmType.Machinegun);
                break;
            case FirearmType.Pistol:
                CurrenGun.Fire(hitPosition, inaccuracity);
                PlayerReferences.playerAnimations.PlayFireAnimation(FirearmType.Pistol);
                break;
            case FirearmType.Burst:
            case FirearmType.Sniper:
            case FirearmType.Shotgun:
                CurrenGun.Fire(hitPosition, inaccuracity);
                break;
            case FirearmType.Knife:
                CurrenGun.KnifeFire();//if you need add your custom fire launcher in networkgun
                PlayerReferences.playerAnimations.PlayFireAnimation(FirearmType.Knife);
                break;
            default:
                Debug.LogWarning("Not defined weapon type to sync bullets.");
                break;
        }
    }

    public void SyncCustomProjectile(Hashtable data) => photonView.RPC(nameof(RPCFireCustom), RpcTarget.Others, data);

    [PunRPC]
    void RPCFireCustom(Hashtable data)
    {
        CurrenGun?.FireCustomLogic(data);
        if (CurrenGun != null)
            PlayerReferences.playerAnimations.PlayFireAnimation(CurrenGun.Info.Type);
    }

    [PunRPC]
    void FireGrenadeRpc(float m_spread, Vector3 pos, Quaternion rot, Vector3 direction)
    {
        CurrenGun.GrenadeFire(m_spread, pos, rot, direction);
    }

    public void SetActiveGrenade(bool active)
    {
        photonView.RPC(nameof(SyncOffAmmoGrenade), RpcTarget.Others, active);
    }

    [PunRPC]
    void SyncOffAmmoGrenade(bool active)
    {
        if (CurrenGun == null)
        {
            Debug.LogError("Grenade is not active on TPS Player");
            return;
        }
        CurrenGun.DesactiveGrenade(active, InvicibleMat);
    }

    public void SetWeaponBlocked(int blockState)
    {
        isWeaponBlocked = blockState == 1;
        if (!PhotonNetwork.OfflineMode)
            photonView.RPC(nameof(RPCSetWBlocked), RpcTarget.Others, blockState);
        else
            RPCSetWBlocked(blockState);
    }

    [PunRPC]
    public void RPCSetWBlocked(int blockState)
    {
        isWeaponBlocked = blockState == 1;
        if (isWeaponBlocked)
        {
            for (int i = 0; i < NetworkGuns.Count; i++)
            {
                NetworkGuns[i].gameObject.SetActive(false);
            }
        }
        else
        {
            CurrentTPVGun(false, true);
        }

        PlayerReferences.playerAnimations.BlockWeapons(blockState);
        currentGunID = -1;
    }

    [PunRPC]
    void SyncCustomizer(int weaponID, string line, PhotonMessageInfo info)
    {
        if (photonView.ViewID != info.photonView.ViewID) return;
    }

    void UpdatePosition()
    {
        if (m_PositionModel.SynchronizeEnabled == false || _receivedNetworkUpdate == false)
        {
            return;
        }

        _transform.localPosition = _positionControl.UpdatePosition(_transform.localPosition);
    }
    void UpdateRotation()
    {
        if (m_RotationModel.SynchronizeEnabled == false || _receivedNetworkUpdate == false)
        {
            return;
        }

        _transform.localRotation = _rotationControl.GetRotation(_transform.localRotation);
    }

    void DoDrawEstimatedPositionError()
    {
        if (NetworkBodyState == PlayerState.InVehicle) return;

        Vector3 targetPosition = _positionControl.GetNetworkPosition();

        Debug.DrawLine(targetPosition, _transform.position, Color.red, 2f);
        Debug.DrawLine(_transform.position, _transform.position + Vector3.up, Color.green, 2f);
        Debug.DrawLine(targetPosition, targetPosition + Vector3.up, Color.red, 2f);
    }

    // 使用插值模式或外推模式 SynchronizeValues 时，这些值会同步到远程对象。移动脚本应传入当前速度（单位/秒）和转向速度（角度/秒），远程对象用它们预测物体运动。
    // 物体当前移动向量，单位/秒。
    // 物体当前转向速度，角度/秒。
    public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
    {
        _positionControl.SetSynchronizedValues(speed, turnSpeed);
    }

    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPCSetWBlocked), RpcTarget.Others, isWeaponBlocked ? 1 : 0);
        }
    }

    private Vector3 GetHeadLookPosition()
    {
        if (PlayerReferences == null || PlayerReferences.playerIK == null)
            return HeadTarget.position + HeadTarget.forward;

        return PlayerReferences.playerIK.HeadLookTarget.position;
    }

    public Transform NetGunsRoot
    {
        get
        {
            if (!GameConfig.Instance.DropGunOnDeath) { CurrentTPVGun(true); }
            var ng = NetworkGuns.FirstOrDefault(x => x != null);
            if (ng != null) return ng.transform.parent;

            return null;
        }
    }

    private PlayerReferencesHandler _playerReferences;
    public PlayerReferencesHandler PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponent<PlayerReferencesHandler>();
            return _playerReferences;
        }
    }
    private FirstPersonControllerBase fpControler => PlayerReferences.firstPersonController;
    private PlayerHealthManagerBase playerHealthManager => PlayerReferences.playerHealthManager;
    private CharacterController characterController => PlayerReferences.characterController;
}