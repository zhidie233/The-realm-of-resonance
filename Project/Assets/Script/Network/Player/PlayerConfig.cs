using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerConfig : NetworkHelper
{

    #region Public members
    public Team PlayerTeam = Team.None;
    public List<MonoBehaviour> RemoteOnlyScripts = new List<MonoBehaviour>();
    public List<MonoBehaviour> LocalOnlyScripts = new List<MonoBehaviour>();
    [Space(5)]
    public GameObject LocalObjects;
    public GameObject RemoteObjects;
    [Header("Player References")]
    public Transform FlagPosition;
    public Transform carrierPoint;
    public GameObject AimPositionReference;
    public Mesh directionMesh;

    [Header("Hands Textures")]
    [ScriptableDrawer] public ArmsMaterial armsMaterial;
    private List<ArmsMaterial.MaterialColor> currentWeaponMaterials = new List<ArmsMaterial.MaterialColor>(); 
    #endregion

    void Awake()
    {
        if (!NetworkWrapper.IsConnected || !NetworkWrapper.InRoom) return;
        PlayerTeam = (Team)photonView.InstantiationData[0];
        if (isMine)
        {
            OnLocalPlayer();
        }
        else
        {
            OnRemotePlayer();
        }
    }

    /// <summary>
    /// 仅在远程玩家上调用
    /// </summary>
    public void OnRemotePlayer()
    {
        for (int i = 0; i < LocalOnlyScripts.Count; i++)
        {
            if (LocalOnlyScripts[i] != null)
            {
                 LocalOnlyScripts[i].enabled = false;
            }
        }
        LocalObjects.SetActive(false);
        gameObject.tag = GameCore.REMOTE_PLAYER_TAG;
        gameObject.layer = (int)Mathf.Log(GameConfig.TagsAndLayerSettings.RemotePlayerRootLayer.value, 2);

        //构建玩家数据
        GamePlayer playerData = new GamePlayer()
        {
            Name = photonView.Owner.NickName,
            Team = PlayerTeam,
            Actor = transform,
            isRealPlayer = true,
            isAlive = true,
            ActorView = photonView,
            AimPosition = carrierPoint,
        };

        GameEvents.DispatchRemoteActorChange(new GameEvents.PlayerChangeData()
        {
            PlayerName = photonView.Owner.NickName,
            GameActor = playerData,
            IsAlive = true,
            NetworkView = photonView
        });
    }

    /// <summary>
    /// 仅在本机玩家上调用
    /// </summary>
    public void OnLocalPlayer()
    {
        gameObject.name = NetworkWrapper.NickName;
        for (int i = 0; i < RemoteOnlyScripts.Count; i++)
        {
            if (RemoteOnlyScripts[i] != null)
            {
                RemoteOnlyScripts[i].enabled = false;
            }
        }
        RemoteObjects.SetActive(false);
        gameObject.tag = GameCore.LOCAL_PLAYER_TAG;
        gameObject.layer = GameConfig.TagsAndLayerSettings.GetLocalPlayerLayerIndex();

        armsMaterial?.SelectTeamMaterial(PlayerTeam);
        if (GameConfig.Instance.spawnHandsVisual)
        {
            StartCoroutine(DoSpawnLoop());
        }
        playerReferences.DefaultCameraFOV = (float)GameCore.Settings.GetSettingOf("FOV");
        GameEvents.onGameSettingsChange += OnGameSettingsChanged;
    }

    private void OnDisable()
    {
        SetDeafultWeaponRender();
        if (isMine)
        {
            GameEvents.onGameSettingsChange -= OnGameSettingsChanged;
        }
    }

    private void OnDestroy()
    {
        if (!isMine)
        {
            GameEvents.DispatchRemoteActorChange(new GameEvents.PlayerChangeData()
            {
                PlayerName = gameObject.name,
                GameActor = null,
                IsAlive = false,
            });
        }
    }

    void OnGameSettingsChanged()
    {
        playerReferences.DefaultCameraFOV = (float)GameCore.Settings.GetSettingOf("FOV");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(carrierPoint == null)
        {
            var pa = GetComponent<PlayerRig>().playerAnimations;
            if (pa != null)
            {
                Animator animator = pa.Animator;
                if (animator != null) { carrierPoint = animator.GetBoneTransform(HumanBodyBones.UpperChest); }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (directionMesh == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawMesh(directionMesh, transform.position, transform.rotation, Vector3.one * 0.4f);
    }
#endif

    public void DoSpawnWeaponRenderEffect(Renderer[] renderers)
    {
        if (!GameConfig.Instance.spawnHandsVisual) return;
        if (currentWeaponMaterials.Count > 0)
        {
            //设置默认颜色
            foreach (var item in currentWeaponMaterials)
            {
                if (item.Material == null) continue;
                item.Material.color = item.Color;
            }
        }
        currentWeaponMaterials.Clear();
        foreach (var item in renderers)
        {
            if (item == null) continue;
            Material[] mats = item.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || !mats[i].HasProperty("_Color")) continue;
                ArmsMaterial.MaterialColor mc = new ArmsMaterial.MaterialColor(mats[i]);
                currentWeaponMaterials.Add(mc);
            }
        }
    }

    IEnumerator DoSpawnLoop()
    {
        float d = 0;
        float st = GameConfig.Instance.spawnShieldTime;
        float value = 0;
        Color teamColor = PlayerTeam.GetTeamColor();
        while (d < 1)
        {
            d += Time.deltaTime / st;
            value = Mathf.PingPong(Time.time, 0.25f) * 4;
            if (currentWeaponMaterials.Count > 0)
            {
                for (int i = 0; i < currentWeaponMaterials.Count; i++)
                {
                    if (currentWeaponMaterials[i].Material == null) continue;
                    currentWeaponMaterials[i].Material.color = Color.Lerp(currentWeaponMaterials[i].Color, teamColor, value);
                }
            }
            yield return null;
        }
        SetDeafultWeaponRender();
    }

    void SetDeafultWeaponRender()
    {
        if (currentWeaponMaterials.Count > 0)
        {
            foreach (var item in currentWeaponMaterials)
            {
                item.Material.color = item.Color;
            }
        }
    }

    public bool isLocal { get { return photonView.IsMine; } }
    public PhotonView View { get { return photonView; } }
    public bool isTeamMate { get { return (PlayerTeam == NetworkWrapper.LocalPlayer.GetPlayerTeam() && !isOneTeamMode); } }

    private PlayerRig _playerReferences;
    public PlayerRig playerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponentInParent<PlayerRig>();
            return _playerReferences;
        }
    }

}