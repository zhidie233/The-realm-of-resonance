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
    /// 玩家生命值偏低时的 UI 颜色
    public Gradient HealthColorGradient;
    private Color CurColor = new Color(0, 0, 0);
    [Header("Shake")]
    [ScriptableDrawer] public ShakePreset damageShakerPresent;

    [Header("Effects")]
    public AudioClip[] HitsSound;
    [SerializeField] private AudioClip[] InjuredSounds = null;

    private bool m_autoHealthRecovery = false;
    public bool autoHealthRecovery { get => m_autoHealthRecovery; set => m_autoHealthRecovery = value; }
    #endregion

    #region Private members
    private TextMeshProUGUI HealthTextUI;
    private Image HealthBar;
    const string FallMethod = "FallDown";
    private CharacterController m_CharacterController;
    private bool isDead = false;
    private string lastDamageGiverActor;
    private int KillReward, ScorePerHeadshot;
    private PlayerNetworkHandler PlayerSync;
    private float TimeToRegenerate = 4;
    private FirearmInventory GunManager;
    private bool isSuscribed = false;
    private int protecTime = 0;
    private RepeatHitDamage repeatHitDamage;
    private CanvasGroup DamageAlpha;
    private float damageAlphaValue, uiFadeDelay = 0;
    private Team thisPlayerTeam = Team.None;
    private bool showIndicator = false;
    private float nextHealthSend = 0;
    private PlayerRig playerReferences;
    private int currentHealth = 100;
    #endregion

    #region Unity Callbacks
    protected override void Awake()
    {
        if (!PhotonNetwork.IsConnected) return;

        base.Awake();
        playerReferences = GetComponent<PlayerRig>();
        m_CharacterController = playerReferences.characterController;
        PlayerSync = playerReferences.playerNetwork;
        GunManager = playerReferences.gunManager;
        DamageAlpha = UIReferences.Instance.PlayerUI.DamageAlpha;
        m_autoHealthRecovery = GameConfig.Instance.autoHealthRecovery;
        protecTime = GameConfig.Instance.spawnShieldTime;
        showIndicator = GameConfig.Instance.hitDirectionIndicator;
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
    /// 调用它对玩家造成一次新伤害
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

        photonView.RPC(nameof(SyncDamage), RpcTarget.AllBuffered, e.Damage, e.From, e.Cause, e.Direction, e.isHeadShot, e.WeaponID, NetworkWrapper.LocalPlayer);
    }
    /// 玩家受到坠落冲击伤害时调用
    public override void DoFallDamage(int damage)
    {
        if (!GameConfig.Instance.fallDamageEnabled || isProtectionEnable || playerReferences.firstPersonController.State == PlayerState.InVehicle || GameController.Instance.GameMatchState == MatchState.Waiting)
            return;

        Vector3 downpos = transform.position - transform.TransformVector(new Vector3(0, 5, 1));
        photonView.RPC(nameof(SyncDamage), RpcTarget.AllBuffered, damage, NetworkWrapper.NickName, DamageCause.FallDamage, downpos, false, 103, NetworkWrapper.LocalPlayer);
    }
    /// 该玩家受到伤害时会在所有客户端调用此函数
    [PunRPC]
    void SyncDamage(int damage, string killer, DamageCause cause, Vector3 hitDirection, bool isHeadshot, int weaponID, Player m_sender)
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
                if (HitsSound.Length > 0)
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

            Die(lastDamageGiverActor, isHeadshot, cause, weaponID, hitDirection, m_sender);
        }
    }
    /// 该玩家在对局中死亡时调用
    /// 此函数判断死因并更新所需统计
	void Die(string killer, bool isHeadshot, DamageCause cause, int weaponID, Vector3 hitPos, Player sender)
    {
        isDead = true;
        transform.parent = null;
        m_CharacterController.enabled = false;
        GameController.Instance.GetGamePlayer(gameObject.name).isAlive = false;
        FirearmData gunInfo = GameConfig.Instance.GetWeapon(weaponID);
        bool isExplosion = gunInfo.Type == FirearmType.Grenade || gunInfo.Type == FirearmType.Launcher;
        playerReferences.onDie?.Invoke();

        if (!isMine)
        {
            playerReferences.playerRagdoll.Ragdolled(new PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                IsFromExplosion = isExplosion,
                AutoDestroy = true
            });
        }
        else
        {
            //让远程玩家掉落武器
            Transform ngr = (GameConfig.Instance.DiscardWeaponOnDeath) ? null : PlayerSync.NetGunsRoot;
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
        if (!isMine)
        {
            //如果杀死该玩家的是本地玩家
            if (lastDamageGiverActor == LocalName)
            {
                AddKill(isHeadshot, weapon, weaponID);
            }
            if (GameConfig.Instance.deathIconsEnabled && !isOneTeamMode)
            {
                if (photonView.Owner.GetPlayerTeam() == NetworkWrapper.LocalPlayer.GetPlayerTeam())
                {
                    ObjectRecyclerBase.Instance.Instantiate("deathicon", transform.position, transform.rotation);
                }
            }
            var mplayer = new GamePlayer(photonView, true, false);
            GameEvents.DispatchRemotePlayerDeath(mplayer);

        }
        else
        {
            //设为可以再次重生
            if (GetGameMode.GetGameModeInfo().onDeath == GameModeSettings.OnDeath.SpawnAfterDelay)
            {
                GameController.Instance.RespawnLocalPlayerAfter();
            }
            if (cause == DamageCause.Bot)
            {
                // 增加本地玩家的死亡数
                if (GameConfig.Instance.botKillCredit == BotKillPolicy.SameAsRealPlayers)
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
                WeaponID = weaponID,
                Target = transform,
                FallbackTarget = playerReferences.playerRagdoll.transform
            }).SetActive(true);
            if (killer == LocalName)
            {
                if (cause == DamageCause.FallDamage)
                {
                    KillReportEntry.Instance.SendTeamHighlightMessage(LocalName, GameTexts.DeathByFall, NetworkWrapper.LocalPlayer.GetPlayerTeam());
                }
                else
                {
                    KillReportEntry.Instance.SendTeamHighlightMessage(LocalName, GameTexts.CommittedSuicide, NetworkWrapper.LocalPlayer.GetPlayerTeam());
                }
            }
            if (GameConfig.Instance.DiscardWeaponOnDeath)
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
                feed.AddData("weaponId", weaponID);
                feed.AddData("headshot", isHeadshot);
                KillReportEntry.Instance.SendKillMessageEvent(feed);
                //这种情况下由本地玩家而不是主客户端更新机器人统计。
                AIManager.Instance.SetBotKill(killer);
            }
            StartCoroutine(DestroyThis());
        }
    }
    /// 获得新击杀时同步并给玩家加分
    public void AddKill(bool isHeadshot, string m_weapon, int weaponID)
    {
        //发送击杀提示消息
        var feed = new KillReportEntry.FeedData()
        {
            LeftText = LocalName,
            RightText = gameObject.name,
            Team = NetworkWrapper.LocalPlayer.GetPlayerTeam()
        };
        feed.AddData("weaponId", weaponID);
        feed.AddData("headshot", isHeadshot);
        KillReportEntry.Instance.SendKillMessageEvent(feed);
        //新增一次击杀并更新玩家信息
        NetworkWrapper.LocalPlayer.PostKill(1);
        // 计算这次击杀获得的得分
        int score = (isHeadshot) ? GameConfig.Instance.matchScore.KillReward + GameConfig.Instance.matchScore.HeadshotExtraReward : GameConfig.Instance.matchScore.KillReward;
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
    /// 循环对玩家持续造成伤害，直到取消。
    public override void DoRepeatHitDamage(RepeatHitDamage info)
    {
        repeatHitDamage = info;
        InvokeRepeating(nameof(MakeRepeatHitDamage), 0, info.Rate);
    }
    /// 从自定义循环中施加伤害
    void MakeRepeatHitDamage()
    {
        if (repeatHitDamage == null)
        {
            CancelRepeatHit();
            return;
        }
        var damageinfo = repeatHitDamage.DamageInfo;
        if (damageinfo == null)
        {
            damageinfo = new DamageInfo();
            damageinfo.Direction = Vector3.zero;
            damageinfo.Cause = DamageCause.Map;
        }
        damageinfo.Damage = repeatHitDamage.Damage;
        GetDamage(damageinfo);
    }
    public override void CancelRepeatHit()
    {
        CancelInvoke(nameof(MakeRepeatHitDamage));
    }
    /// 用该玩家的数据更新玩家生命值 UI
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
        if (!m_autoHealthRecovery || currentHealth < 1) return;
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
    /// 让本地玩家自杀
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
    /// 玩家拾取医疗包时调用此事件
    /// <param name="amount"> 在当前生命值上增加的数量</param>
    void OnPickUp(int amount)
    {
        SetHealth(amount);
    }
    /// 为该玩家增加生命值并同步给所有客户端
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
    /// 拾取医疗包时同步生命值。
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
    /// 回合结束时
    /// 停用部分功能
    void OnRoundEnd()
    {
        DamageEnabled = false;
    }
}
