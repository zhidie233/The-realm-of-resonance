using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

[RequireComponent(typeof(PhotonView))]
public class bl_PlayerNetwork : bl_MonoBehaviour, IPunObservable
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
    public List<bl_NetworkGun> NetworkGuns = new List<bl_NetworkGun>();
    [SerializeField]
    PhotonTransformViewPositionModel m_PositionModel = new PhotonTransformViewPositionModel();
    [SerializeField]
    PhotonTransformViewRotationModel m_RotationModel = new PhotonTransformViewRotationModel();
    public Material InvicibleMat;

    private bl_NamePlateBase DrawName;
    private bool SendInfo = false;
    private bool isWeaponBlocked = false;
    private bl_NetworkGun currentGun;
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
#if UMM
    private bl_MiniMapItem MiniMapItem = null;
#endif

    // 此玩家是否为本地玩家的队友
    public bool isFriend { get; set; }

    // 当前第三人称武器 ID
    public int networkGunID { get; set; }

    // 此玩家手中的当前第三人称武器
    public bl_NetworkGun CurrenGun { get; set; }

    // 从此玩家客户端侧接收的当前状态
    public PlayerState NetworkBodyState { get; set; }

    protected override void Awake()
    {
        base.Awake();
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;

        _transform = transform;
        _positionControl = new PhotonTransformViewPositionControl(m_PositionModel);
        _rotationControl = new PhotonTransformViewRotationControl(m_RotationModel);
        DrawName = GetComponent<bl_NamePlateBase>();
        NetworkGuns.ForEach(x =>
        {
            if (x != null) x.gameObject.SetActive(false);
        });
#if UMM
        MiniMapItem = this.GetComponent<bl_MiniMapItem>();
        if (isMine && MiniMapItem != null)
        {
            MiniMapItem.enabled = false;
        }
#endif
    }

    private void Start()
    {
        InvokeRepeating(nameof(SlowLoop), 0, 1);
    }

    // serialization method of photon
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
            //We own this player: send the others our data
            stream.SendNext(GetHeadLookPosition());
            stream.SendNext(fpControler.State);
            stream.SendNext(fpControler.isGrounded);
            stream.SendNext((byte)PlayerReferences.gunManager.GetCurrentGunID);//send as byte, max value is 255.
            stream.SendNext(FPState);
            stream.SendNext(fpControler.Velocity);
        }
        else
        {
            //Network player, receive data
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
        if (bl_GameData.Instance.DropGunOnDeath && bl_RoomMenu.Instance != null && !bl_RoomMenu.Instance.isApplicationQuitting)
        {
            if (NetGunsRoot != null) NetGunsRoot.gameObject.SetActive(false);
        }
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
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

    // Function called each frame when the player is local
    void OnLocalPlayer()
    {
        //send the state of player local for remote animation
        PlayerReferences.playerAnimations.BodyState = fpControler.State;
        PlayerReferences.playerAnimations.IsGrounded = fpControler.isGrounded;
        PlayerReferences.playerAnimations.Velocity = fpControler.Velocity;
        PlayerReferences.playerAnimations.FPState = FPState;
    }

    // Function called each frame when the player is remote
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
            //Determine if remote player is teamMate or enemy
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

    // Function called each frame when this Remote player is an enemy
    void OnEnemyPlayer()
    {
        playerHealthManager.DamageEnabled = true;
        DrawName.SetActive(bl_SpectatorModeBase.Instance.isActive);
#if UMM
        if (bl_MiniMapData.Instance.showEnemysWhenFire)
        {
#if KSA
            if (bl_KillStreakHandler.Instance != null && bl_KillStreakHandler.Instance.activeAUVs > 0) return;
#endif
            if (FPState == PlayerFPState.Firing || FPState == PlayerFPState.FireAiming)
            {
                MiniMapItem?.ShowItem();
            }
            else
            {
                MiniMapItem?.HideItem();
            }
        }
#endif
    }

    // Function called each frame when this Remote player is a teammate.
    void OnTeammatePlayer()
    {
        playerHealthManager.DamageEnabled = bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire;
        DrawName.SetActive(true);
        characterController.enabled = false;

        if (!SendInfo)
        {
            SendInfo = true;
            PlayerReferences.playerRagdoll.IgnorePlayerCollider();
        }

#if UMM
        MiniMapItem?.ShowItem();
#endif
    }

    // This function control which TP Weapon should be showing on remote players.
    // Should take the current gun from the local player or from the network data?
    // Double-check even if the equipped weapon has not changed.
    public void CurrentTPVGun(bool local = false, bool force = false)
    {
        if (PlayerReferences.gunManager == null)
            return;
        if (networkGunID == currentGunID && !force) return;

        //Get the current gun ID local and sync with remote
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
        isFriend = (RemoteTeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    // Change the current TPWeapon on this remote player.
    public void SetNetworkWeapon(GunType weaponType, bl_NetworkGun networkGun)
    {
        PlayerReferences.playerAnimations?.SetNetworkGun(weaponType, networkGun);
    }

    // Send a call to all other clients to sync a bullet
    public void ReplicateFire(GunType weaponType, Vector3 hitPosition, Vector3 inacuracity)
    {
        photonView.RPC(nameof(FireSync), RpcTarget.Others, weaponType, hitPosition, inacuracity);
#if MFPSTPV
        if (bl_CameraViewSettings.IsThirdPerson())
            PlayerReferences.playerAnimations?.PlayFireAnimation(weaponType);
#endif
    }

    // public method to send the RPC shot synchronization
    public void IsFireGrenade(float t_spread, Vector3 pos, Quaternion rot, Vector3 direction)
    {
        photonView.RPC(nameof(FireGrenadeRpc), RpcTarget.Others, new object[] { t_spread, pos, rot, direction });
    }

    // Synchronize the shot with the current remote weapon send the information necessary so that fire impact in the same direction as the local
    [PunRPC]
    void FireSync(GunType weaponType, Vector3 hitPosition, Vector3 inaccuracity)
    {
        if (CurrenGun == null) return;
        switch (weaponType)
        {
            case GunType.Machinegun:
                CurrenGun.Fire(hitPosition, inaccuracity);
                PlayerReferences.playerAnimations.PlayFireAnimation(GunType.Machinegun);
                break;
            case GunType.Pistol:
                CurrenGun.Fire(hitPosition, inaccuracity);
                PlayerReferences.playerAnimations.PlayFireAnimation(GunType.Pistol);
                break;
            case GunType.Burst:
            case GunType.Sniper:
            case GunType.Shotgun:
                CurrenGun.Fire(hitPosition, inaccuracity);
                break;
            case GunType.Knife:
                CurrenGun.KnifeFire();//if you need add your custom fire launcher in networkgun
                PlayerReferences.playerAnimations.PlayFireAnimation(GunType.Knife);
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

    // <param name="blockState"></param>
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
#if CUSTOMIZER
        bl_NetworkGun ng = NetworkGuns.Find(x => x.LocalGun.GunID == weaponID);
        if (ng != null)
        {
            if (ng.GetComponent<bl_CustomizerWeapon>() != null)
            {
                ng.GetComponent<bl_CustomizerWeapon>().ApplyAttachments(line);
            }
            else
            {
                Debug.LogWarning("You have not setup the attachments in the TPWeapon: " + weaponID);
            }
        }
#endif
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

    // These values are synchronized to the remote objects if the interpolation mode or the extrapolation mode SynchronizeValues is used. Your movement script should pass on the current speed (in units/second) and turning speed (in angles/second) so the remote object can use them to predict the objects movement.
    // The current movement vector of the object in units/second.
    // The current turn speed of the object in angles/second.
    public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
    {
        _positionControl.SetSynchronizedValues(speed, turnSpeed);
    }

    // <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPCSetWBlocked), RpcTarget.Others, isWeaponBlocked ? 1 : 0);
        }
    }

    // <returns></returns>
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
            if (!bl_GameData.Instance.DropGunOnDeath) { CurrentTPVGun(true); }
            var ng = NetworkGuns.FirstOrDefault(x => x != null);
            if (ng != null) return ng.transform.parent;

            return null;
        }
    }

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }
    private bl_FirstPersonControllerBase fpControler => PlayerReferences.firstPersonController;
    private bl_PlayerHealthManagerBase playerHealthManager => PlayerReferences.playerHealthManager;
    private CharacterController characterController => PlayerReferences.characterController;
}