using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthManager : PlayerHealthManagerBase
{
    #region Public members
    [Header("Settings")]
    [Range(0, 100)] public int health = 100;
    [Range(1, 100)] public int maxHealth = 100;
    [Range(1, 10)] public float StartRegenerateIn = 4f;
    [Range(1, 5)] public float RegenerationSpeed = 3f;
    [Range(10, 100)] public int RegenerateUpTo = 100;

    [Header("GUI")]
    /// <summary>
    /// 玩家生命值偏低时的 UI 颜色
    /// </summary>
    public Gradient HealthColorGradient;
    private Color CurColor = new Color(0, 0, 0);
    [Header("Shake")]
    [ScriptableDrawer] public ShakePreset damageShakerPresent;

    [Header("Effects")]
    public AudioClip[] HitsSound;
    [SerializeField] private AudioClip[] InjuredSounds = null;

    private bool m_HealthRegeneration = false;
    public bool HealthRegeneration { get => m_HealthRegeneration; set => m_HealthRegeneration = value; }
    #endregion

    #region Private members
    private TextMeshProUGUI HealthTextUI;
    private Image HealthBar;
    const string FallMethod = "FallDown";
    private CharacterController m_CharacterController;
    private bool isDead = false;
    private string lastDamageGiverActor;
    private int ScorePerKill, ScorePerHeatShot;
    private PlayerNetworkHandler PlayerSync;
    private float TimeToRegenerate = 4;
    private FirearmInventory GunManager;
    private bool isSuscribed = false;
    private int protecTime = 0;
    private RepetingDamageInfo repetingDamageInfo;
    private CanvasGroup DamageAlpha;
    private float damageAlphaValue, uiFadeDelay = 0;
    private Team thisPlayerTeam = Team.None;
    private bool showIndicator = false;
    private float nextHealthSend = 0;
    private PlayerReferencesHandler playerReferences;
    private int currentHealth = 100;
    #endregion

    #region Unity Callbacks
    protected override void Awake()
    {
        if (!PhotonNetwork.IsConnected) return;

        base.Awake();
        playerReferences = GetComponent<PlayerReferencesHandler>();
        m_CharacterController = playerReferences.characterController;
        PlayerSync = playerReferences.playerNetwork;
        GunManager = playerReferences.gunManager;
        DamageAlpha = UIReferences.Instance.PlayerUI.DamageAlpha;
        m_HealthRegeneration = GameConfig.Instance.HealthRegeneration;
        protecTime = GameConfig.Instance.SpawnProtectedTime;
        showIndicator = GameConfig.Instance.showDamageIndicator;
    }

    void Start()
    {
        if (!isConnected)
            return;

        currentHealth = health;
        thisPlayerTeam = (Team)photonView.InstantiationData[0];
        if (isMine)
        {
            GameCore.LocalPlayer.IsAlive = true;
            gameObject.name = PhotonNetwork.NickName;
            HealthTextUI = UIReferences.Instance.PlayerUI.HealthText;
            HealthBar = UIReferences.Instance.PlayerUI.HealthBar;
            UpdateUI();
        }
        if (protecTime > 0) { InvokeRepeating(nameof(OnProtectCount), 1, 1); }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (this.isMine)
        {
            GameController.LocalPlayerViewID = this.photonView.ViewID;
            GameEvents.onPickUpHealth += this.OnPickUp;
            GameEvents.onRoundEnd += this.OnRoundEnd;
            NetworkCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
            isSuscribed = true;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (isSuscribed)
        {
            GameEvents.onPickUpHealth -= this.OnPickUp;
            GameEvents.onRoundEnd -= this.OnRoundEnd;
            NetworkCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        }
    }

    public override void OnUpdate()
    {
        if (isMine)
        {
            DamageUI();
            RegenerateHealth();
        }
    }
    #endregion

    public override void DoDamage(DamageInfo damageData)
    {
        GetDamage(damageData);
    }

    /// <summary>
    /// 调用它对玩家造成一次新伤害
    /// </summary>
    public void GetDamage(DamageInfo e)
    {
        bool canDamage = true;
        if (!DamageEnabled)
        {
            //修复：机器人无法伤害主客户端的队友。
            if (e.GameActor != null && (e.GameActor.Team != thisPlayerTeam || RoomSettings.Instance.CurrentRoomInfo.friendlyFire)) { canDamage = true; }
            else canDamage = false;
        }

        if (!canDamage || isProtectionEnable || GameController.Instance.GameMatchState == MatchState.Waiting)
        {
            if (!isMine)
            {
                playerReferences.playerAnimations.OnGetHit();
            }
            return;
        }

        photonView.RPC(nameof(SyncDamage), RpcTarget.AllBuffered, e.Damage, e.From, e.Cause, e.Direction, e.isHeadShot, e.GunID, NetworkWrapper.LocalPlayer);
    }

    /// <summary>
    /// 玩家受到坠落冲击伤害时调用
    /// </summary>
    public override void DoFallDamage(int damage)
    {
        if (!GameConfig.Instance.allowFallDamage || isProtectionEnable || playerReferences.firstPersonController.State == PlayerState.InVehicle || GameController.Instance.GameMatchState == MatchState.Waiting)
            return;

        Vector3 downpos = transform.position - transform.TransformVector(new Vector3(0, 5, 1));
        photonView.RPC(nameof(SyncDamage), RpcTarget.AllBuffered, damage, NetworkWrapper.NickName, DamageCause.FallDamage, downpos, false, 103, NetworkWrapper.LocalPlayer);
    }

    /// <summary>
    /// 该玩家受到伤害时会在所有客户端调用此函数
    /// </summary>
    [PunRPC]
    void SyncDamage(int damage, string killer, DamageCause cause, Vector3 hitDirection, bool isHeatShot, int weaponID, Player m_sender)
    {
        if (isDead || isProtectionEnable)
            return;

        if (DamageEnabled)
        {
            if (currentHealth >= 0)
            {
                if (isMine)
                {
                    damageAlphaValue = Mathf.Max(1f - (((float)currentHealth - (float)damage) / (float)maxHealth), 0.25f);
                    uiFadeDelay = StartRegenerateIn;
                    GameEvents.DoPlayerCameraShake(damageShakerPresent, "damage");
                    if (showIndicator) DamageIndicatorBase.Instance?.SetHit(hitDirection);
                    TimeToRegenerate = StartRegenerateIn;
                    GameEvents.DispatchLocalPlayerReceiveDamage(damage);
                }
                else
                {
                    // 伤害由本地玩家造成时
                    if (m_sender != null && m_sender.NickName == LocalName && cause != DamageCause.Bot)
                    {
                        ReticleBase.Instance.OnHit();
                        AudioManager.Instance.PlayClip("body-hit");
                        GameEvents.DispatchLocalPlayerHitEnemy(new GameHitData()
                        {
                            HitTransform = transform,
                            HitPosition = transform.position,
                            Damage = damage,
                            HitName = gameObject.name
                        });
                    }
                }
            }

            if (cause != DamageCause.FallDamage && cause != DamageCause.Fire)
            {
                if (HitsSound.Length > 0)//Audio effect of hit
                {
                    AudioSource.PlayClipAtPoint(HitsSound[Random.Range(0, HitsSound.Length)], transform.position, 1.0f);
                }
            }
            else
            {
                AudioSource.PlayClipAtPoint(InjuredSounds[Random.Range(0, InjuredSounds.Length)], transform.position, 1.0f);
            }

            lastDamageGiverActor = killer;
        }

        if (currentHealth > 0)
        {
            currentHealth -= damage;
            if (!isMine)
            {
                playerReferences.playerAnimations.OnGetHit();
            }
            else
            {
                UpdateUI();
            }
        }

        if (currentHealth < 1)
        {
            currentHealth = 0;

            if (isMine)
            {
                GameCore.LocalPlayer.IsAlive = false;
                GameEvents.DispatchPlayerLocalDeathEvent();
            }

            Die(lastDamageGiverActor, isHeatShot, cause, weaponID, hitDirection, m_sender);
        }
    }

    /// <summary>
    /// 该玩家在对局中死亡时调用
    /// 此函数判断死因并更新所需统计
    /// </summary>
	void Die(string killer, bool isHeadshot, DamageCause cause, int gunID, Vector3 hitPos, Player sender)
    {
        isDead = true;
        transform.parent = null;
        m_CharacterController.enabled = false;
        GameController.Instance.GetGamePlayer(gameObject.name).isAlive = false;
        FirearmData gunInfo = GameConfig.Instance.GetWeapon(gunID);
        bool isExplosion = gunInfo.Type == FirearmType.Grenade || gunInfo.Type == FirearmType.Launcher;
        playerReferences.onDie?.Invoke();

        if (!isMine)
        {
            playerReferences.playerRagdoll.Ragdolled(new PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                IsFromExplosion = isExplosion,
                AutoDestroy = true
            });// convert into ragdoll the remote player
        }
        else
        {
            //让远程玩家掉落武器
            Transform ngr = (GameConfig.Instance.DropGunOnDeath) ? null : PlayerSync.NetGunsRoot;
            playerReferences.playerRagdoll.SetLocalRagdoll(new PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                Velocity = m_CharacterController.velocity,
                IsFromExplosion = isExplosion,
                AutoDestroy = true,
                RightHandChild = ngr
            });
        }

        //禁用玩家预制体其余子对象
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        string weapon = cause.ToString();
        if (cause == DamageCause.Player || cause == DamageCause.Bot || cause == DamageCause.Explosion)
        {
            weapon = gunInfo.Name;
        }

        if (!isMine)// when player is not ours
        {
            //如果杀死该玩家的是本地玩家
            if (lastDamageGiverActor == LocalName)
            {
                AddKill(isHeadshot, weapon, gunID);
            }

            if (GameConfig.Instance.showDeathIcons && !isOneTeamMode)
            {
                if (photonView.Owner.GetPlayerTeam() == NetworkWrapper.LocalPlayer.GetPlayerTeam())
                {
                    ObjectRecyclerBase.Instance.Instantiate("deathicon", transform.position, transform.rotation);
                }
            }

            var mplayer = new GamePlayer(photonView, true, false);
            GameEvents.DispatchRemotePlayerDeath(mplayer);
        }
        else//when is local player who dies
        {

            //设为可以再次重生
            if (GetGameMode.GetGameModeInfo().onPlayerDie == GameModeSettings.OnPlayerDie.SpawnAfterDelay)
            {
                GameController.Instance.RespawnLocalPlayerAfter();
            }

            if (cause == DamageCause.Bot)
            {
                // 增加本地玩家的死亡数
                if (GameConfig.Instance.howConsiderBotsEliminations == BotKillPolicy.SameAsRealPlayers)
                    NetworkWrapper.LocalPlayer.PostDeaths(1);
            }
            else
            {
                // 增加本地玩家的死亡数
                NetworkWrapper.LocalPlayer.PostDeaths(1);
            }

            playerReferences.playerRagdoll.gameObject.name = "YOU";

            //显示击杀摄像机
            DeathCamBase.Instance.SetTarget(new DeathCamBase.DeathCamInfo()
            {
                RealPlayer = sender,
                TargetName = killer,
                GunID = gunID,
                Target = transform,
                FallbackTarget = playerReferences.playerRagdoll.transform
            }).SetActive(true);


            if (killer == LocalName)
            {
                if (cause == DamageCause.FallDamage)
                {
                    KillReportEntry.Instance.SendTeamHighlightMessage(LocalName, GameTexts.DeathByFall.Localized(20), NetworkWrapper.LocalPlayer.GetPlayerTeam());

                }
                else
                {
                    KillReportEntry.Instance.SendTeamHighlightMessage(LocalName, GameTexts.CommittedSuicide.Localized(19), NetworkWrapper.LocalPlayer.GetPlayerTeam());
                }
            }

            if (GameConfig.Instance.DropGunOnDeath)
            {
                GunManager.ThrowCurrent(true, PlayerSync.NetGunsRoot.position + transform.forward);
            }

            if (cause == DamageCause.Bot)
            {
                var feed = new KillReportEntry.FeedData()
                {
                    LeftText = killer,
                    RightText = gameObject.name,
                    Team = NetworkWrapper.LocalPlayer.GetPlayerTeam()
                };
                feed.AddData("gunid", gunID);
                feed.AddData("headshot", isHeadshot);

                KillReportEntry.Instance.SendKillMessageEvent(feed);
                //这种情况下由本地玩家而不是主客户端更新机器人统计。
                AIManager.Instance.SetBotKill(killer);
            }
            StartCoroutine(DestroyThis());
        }
    }

    /// <summary>
    /// 获得新击杀时同步并给玩家加分
    /// </summary>
    public void AddKill(bool isHeadshot, string m_weapon, int gunID)
    {
        //发送击杀提示消息
        var feed = new KillReportEntry.FeedData()
        {
            LeftText = LocalName,
            RightText = gameObject.name,
            Team = NetworkWrapper.LocalPlayer.GetPlayerTeam()
        };
        feed.AddData("gunid", gunID);
        feed.AddData("headshot", isHeadshot);

        KillReportEntry.Instance.SendKillMessageEvent(feed);

        //新增一次击杀并更新玩家信息
        NetworkWrapper.LocalPlayer.PostKill(1);
        // 计算这次击杀获得的得分
        int score = (isHeadshot) ? GameConfig.Instance.ScoreReward.ScorePerKill + GameConfig.Instance.ScoreReward.ScorePerHeadShot : GameConfig.Instance.ScoreReward.ScorePerKill;

        //显示本地击杀通知
        var localKillInfo = new KillRecord();
        localKillInfo.Killer = lastDamageGiverActor;
        localKillInfo.Killed = gameObject.name;
        localKillInfo.byHeadShot = isHeadshot;
        localKillInfo.KillMethod = m_weapon;
        GameEvents.DispatchLocalKillEvent(localKillInfo);

        var elimatedTeam = photonView.Owner.GetPlayerTeam();
        if (isOneTeamMode)
        {
            NetworkWrapper.LocalPlayer.PostScore(score);
        }
        else if (elimatedTeam != Team.All && elimatedTeam != NetworkWrapper.LocalPlayer.GetPlayerTeam())
        {
            NetworkWrapper.LocalPlayer.PostScore(score);
        }

        GameController.Instance.OnLocalPlayerKill();

    }

    /// <summary>
    /// 循环对玩家持续造成伤害，直到取消。
    /// </summary>
    public override void DoRepetingDamage(RepetingDamageInfo info)
    {
        repetingDamageInfo = info;
        InvokeRepeating(nameof(MakeDamageRepeting), 0, info.Rate);
    }

    /// <summary>
    /// 从自定义循环中施加伤害
    /// </summary>
    void MakeDamageRepeting()
    {
        if (repetingDamageInfo == null)
        {
            CancelRepetingDamage();
            return;
        }

        var damageinfo = repetingDamageInfo.DamageInfo;
        if (damageinfo == null)
        {
            damageinfo = new DamageInfo();
            damageinfo.Direction = Vector3.zero;
            damageinfo.Cause = DamageCause.Map;
        }
        damageinfo.Damage = repetingDamageInfo.Damage;

        GetDamage(damageinfo);
    }

    public override void CancelRepetingDamage()
    {
        CancelInvoke(nameof(MakeDamageRepeting));
    }

    /// <summary>
    /// 用该玩家的数据更新玩家生命值 UI
    /// </summary>
    void UpdateUI()
    {
        float h = Mathf.Max(currentHealth, 0);
        float deci = h * 0.01f;
        CurColor = HealthColorGradient.Evaluate(deci);
        if (HealthTextUI != null)
        {
            HealthTextUI.text = Mathf.FloorToInt(currentHealth).ToString();
            HealthTextUI.color = CurColor;
        }
        if (HealthBar != null) { HealthBar.fillAmount = deci; HealthBar.color = CurColor; }
    }

    void DamageUI()
    {
        if (damageAlphaValue <= 0)
        {
            DamageAlpha.alpha = 0;
            return;
        }

        DamageAlpha.alpha = Mathf.Lerp(DamageAlpha.alpha, damageAlphaValue, Time.deltaTime * 6);

        if (uiFadeDelay <= 0)
        {
            damageAlphaValue -= Time.deltaTime;
        }
        else
        {
            uiFadeDelay -= Time.deltaTime;
        }
    }

    void RegenerateHealth()
    {
        if (!m_HealthRegeneration || currentHealth < 1) return;
        if (currentHealth >= RegenerateUpTo) return;

        int projectedHealth = currentHealth;
        if (TimeToRegenerate <= 0)
        {
            projectedHealth = currentHealth;
            projectedHealth += 1;
            uiFadeDelay = 0;
        }
        else
        {
            TimeToRegenerate -= Time.deltaTime * 1.15f;
        }

        if (Time.time - nextHealthSend >= (1 / RegenerationSpeed))
        {
            nextHealthSend = Time.time;
            photonView.RPC(nameof(PickUpHealth), RpcTarget.All, projectedHealth);
        }
        UpdateUI();
    }

    /// <summary>
    /// 让本地玩家自杀
    /// </summary>
    public override bool Suicide()
    {
        if (!isMine || !GameCore.LocalPlayer.IsAlive) return false;
        if (isProtectionEnable) return false;

        DamageInfo e = new DamageInfo();
        e.Damage = 500;
        e.From = base.LocalName;
        e.Direction = transform.position;
        e.isHeadShot = false;
        GetDamage(e);
        return true;
    }

    void OnProtectCount()
    {
        protecTime--;
        if (isMine)
        {
            UIReferences.Instance.OnSpawnCount(protecTime);
        }
        if (protecTime <= 0)
        {
            CancelInvoke(nameof(OnProtectCount));
        }
    }

    IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(0.3f);
        DestroyEntity();
    }

    public override void DestroyEntity()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

    /// <summary>
    /// 玩家拾取医疗包时调用此事件
    /// </summary>
    /// <param name="amount"> 在当前生命值上增加的数量</param>
    void OnPickUp(int amount)
    {
        SetHealth(amount);
    }

    /// <summary>
    /// 为该玩家增加生命值并同步给所有客户端
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="replace"></param>
    public override void SetHealth(int amount, bool replace = false)
    {
        if (currentHealth < 1 || isDead) return;

        if (photonView.IsMine)
        {
            int newHealth = currentHealth + amount;
            if (replace) newHealth = amount;

            currentHealth = newHealth;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
                damageAlphaValue = 1;
            }
            uiFadeDelay = 0;
            UpdateUI();
            photonView.RPC(nameof(PickUpHealth), RpcTarget.OthersBuffered, newHealth);
        }
    }

    [PunRPC]
    void RpcSyncHealth(int newHealth, PhotonMessageInfo info)
    {
        if (info.photonView.ViewID == photonView.ViewID)
        {
            currentHealth = (int)newHealth;
        }
    }

    /// <summary>
    /// 拾取医疗包时同步生命值。
    /// </summary>
    [PunRPC]
    void PickUpHealth(int t_amount)
    {
        if (currentHealth < 1 || isDead) return;

        currentHealth = (int)t_amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    private bool isProtectionEnable { get { return (protecTime > 0); } }

    public override int GetHealth() => currentHealth;

    public override int GetMaxHealth() => maxHealth;

    public override bool IsDeath()
    {
        return isDead;
    }

    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RpcSyncHealth), newPlayer, (int)currentHealth);
        }
    }

    /// <summary>
    /// 回合结束时
    /// 停用部分功能
    /// </summary>
    void OnRoundEnd()
    {
        DamageEnabled = false;
    }
}