using Game.Audio;
using Game.Runtime.AI;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Handle all relating to the bot health
/// This script has not direct references, you can replace with your own script
/// Simply make sure to inherit your script from <see cref="PlayerHealthManagerBase"/>
/// </summary>
public class AIShooterHealth : PlayerHealthManagerBase
{

    [Range(10, 500)] public int Health = 100;

    #region Private members
    private AIShooter m_AIShooter;
    private int LastActorEnemy = -1;
    private RepetingDamageInfo repetingDamageInfo;
    private AIShooterReferences references;
    private AIShooter shooterAgent;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        references = GetComponent<AIShooterReferences>();
        m_AIShooter = references.aiShooter;
        shooterAgent = GetComponent<AIShooter>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageData"></param>
    public override void DoDamage(DamageData damageData)
    {
        string weaponName = GameConfig.Instance.GetWeapon(damageData.GunID).Name;
        //if was not killed by a listed weapon
        if (damageData.GunID < 0)
        {
            //try to find the custom weapon name
            var iconData = KillFeedBase.Instance.GetCustomIconByIndex(Mathf.Abs(damageData.GunID + 1));
            if (iconData != null)
            {
                weaponName = $"cmd:{iconData.Name}";
            }
        }

        DoDamage(damageData.Damage, weaponName, damageData.Direction, damageData.ActorViewID, !damageData.GameActor.isRealPlayer, damageData.GameActor.Team, damageData.isHeadShot);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    public override void DoFallDamage(int damage)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoDamage(int damage, string weaponName, Vector3 direction, int viewID, bool fromBot, Team team, bool ishead)
    {
        if (m_AIShooter.isDeath)
            return;

        if (!isOneTeamMode)
        {
            if (!RoomSettings.Instance.CurrentRoomInfo.friendlyFire && team == m_AIShooter.AITeam) return;
        }

        photonView.RPC(nameof(RpcDoDamage), RpcTarget.All, damage, weaponName, direction, viewID, fromBot, ishead);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="weaponName"></param>
    /// <param name="direction"></param>
    /// <param name="viewID"></param>
    /// <param name="fromBot"></param>
    /// <param name="ishead"></param>
    [PunRPC]
    void RpcDoDamage(int damage, string weaponName, Vector3 direction, int viewID, bool fromBot, bool ishead)
    {
        if (m_AIShooter.isDeath)
            return;

        Health -= damage;
        if (LastActorEnemy != viewID)
        {
            if (shooterAgent != null)
                shooterAgent.FocusOnSingleTarget = false;
        }
        LastActorEnemy = viewID;

        if (PhotonNetworkWrapper.IsMasterClient)
        {
            shooterAgent?.OnGetHit(direction);
        }
        if (viewID == GameController.LocalPlayerViewID)//if was me that make damage
        {
            CrosshairBase.Instance.OnHit();
            AudioManager.Instance.PlayClip("body-hit");
            GameEvents.DispatchLocalPlayerHitEnemy(new GameHitData()
            {
                HitTransform = transform,
                HitPosition = transform.position,
                Damage = damage,
                HitName = gameObject.name
            });
        }

        if (Health > 0)
        {
            Transform t = GameController.Instance.FindActor(viewID);
            if (t != null && !t.name.Contains("(die)"))
            {
                if (m_AIShooter.Target == null)
                {
                    if (shooterAgent != null)
                        shooterAgent.FocusOnSingleTarget = true;
                    m_AIShooter.Target = t;
                }
                else
                {
                    if (t != m_AIShooter.Target)
                    {
                        float cd = UtilityHelper.Distance(transform.position, m_AIShooter.Target.position);
                        float od = UtilityHelper.Distance(transform.position, t.position);
                        if (od < cd && (cd - od) > 7)
                        {
                            if (shooterAgent != null)
                                shooterAgent.FocusOnSingleTarget = true;
                            m_AIShooter.Target = t;
                        }
                    }
                }
            }
            references.aiAnimation.OnGetHit();
        }
        else
        {
            Die(viewID, fromBot, ishead, weaponName, direction);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Die(int viewID, bool fromBot, bool ishead, string weaponName, Vector3 direction)
    {
        //Debug.Log($"{gameObject.name} die with {weaponName} from viewID {viewID} Bot?= {fromBot}");
        m_AIShooter.isDeath = true;
        m_AIShooter.OnDeath();
        gameObject.name += " (die)";
        m_AIShooter.AimTarget.name += " (die)";
        references.shooterWeapon.OnDeath();
        m_AIShooter.enabled = false;
        references.onDie?.Invoke();

        if (PhotonNetworkWrapper.IsMasterClient && references.Agent.enabled) references.Agent.isStopped = true;
        GetComponent<NamePlateBase>().SetActive(false);
        //update the GamePlayer data
        GamePlayer player = GameController.Instance.GetGamePlayer(m_AIShooter.AIName);
        if (player != null)
            player.isAlive = false;

        AIShooter killerBot = null;
        //if was local player who terminated this bot
        if (viewID == GameController.LocalPlayerViewID && !fromBot)
        {
            Team team = PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam();
            //send kill feed message
            int gunID = GameConfig.Instance.GetWeaponID(weaponName);
            if (weaponName.Contains("cmd:") || gunID == -1)
            {
                weaponName = weaponName.Replace("cmd:", "");
                gunID = -(KillFeedBase.Instance.GetCustomIconIndex(weaponName) + 1);
            }

            var feed = new KillFeedBase.FeedData()
            {
                LeftText = LocalName,
                RightText = m_AIShooter.AIName,
                Team = team
            };
            feed.AddData("gunid", gunID);
            feed.AddData("headshot", ishead);

            KillFeedBase.Instance.SendKillMessageEvent(feed);

            var killConsideration = GameConfig.Instance.howConsiderBotsEliminations;
            if (killConsideration != BotKillConsideration.DoNotCountAtAll)
            {
                if (isOneTeamMode)
                {
                    PhotonNetworkWrapper.LocalPlayer.PostKill(1);
                    RoomSettings.IncreaseMatchPersistData("bot-kills", 1);
                }
                else if (shooterAgent.AITeam != Team.All && shooterAgent.AITeam != GameCore.LocalPlayer.Team)
                {
                    PhotonNetworkWrapper.LocalPlayer.PostKill(1);
                    RoomSettings.IncreaseMatchPersistData("bot-kills", 1);
                }
            }

            // only grant score to the kill player if bot eliminations counts as real players.
            if (killConsideration == BotKillConsideration.SameAsRealPlayers)
            {
                int score;
                //If headshot will give you double experience
                if (ishead)
                {
                    GameController.Instance.Headshots++;
                    score = GameConfig.Instance.ScoreReward.ScorePerKill + GameConfig.Instance.ScoreReward.ScorePerHeadShot;
                }
                else
                {
                    score = GameConfig.Instance.ScoreReward.ScorePerKill;
                }

                if (isOneTeamMode) PhotonNetworkWrapper.LocalPlayer.PostScore(score);
                else if (shooterAgent.AITeam != Team.All && shooterAgent.AITeam != GameCore.LocalPlayer.Team)
                {
                    PhotonNetworkWrapper.LocalPlayer.PostScore(score);
                }
            }

            //show an local notification for the kill
            var localKillInfo = new KillInfo();
            localKillInfo.Killer = PhotonNetworkWrapper.LocalPlayer.NickName;
            localKillInfo.Killed = string.IsNullOrEmpty(m_AIShooter.AIName) ? gameObject.name.Replace("(die)", "") : m_AIShooter.AIName;
            localKillInfo.byHeadShot = ishead;
            localKillInfo.KillMethod = weaponName;
            GameEvents.DispatchLocalKillEvent(localKillInfo);

            //update team score
            GameController.Instance.SetPoint(1, GameMode.TDM, team);

        }
        //if was killed by another bot
        else if (fromBot)
        {
            //make Master handle as the owner
            if (PhotonNetworkWrapper.IsMasterClient)
            {
                //find the killer in the scene
                PhotonView p = PhotonView.Find(viewID);
                AIShooter bot = null;
                string killer = "Unknown";
                if (p != null)
                {
                    bot = p.GetComponent<AIShooter>();//killer bot
                    killer = bot.AIName;
                    if (string.IsNullOrEmpty(killer)) { killer = p.gameObject.name.Replace(" (die)", ""); }
                    //update bot stats
                    AIMananger.Instance.SetBotKill(killer);
                }

                //send kill feed message
                int gunID = GameConfig.Instance.GetWeaponID(weaponName);

                var feed = new KillFeedBase.FeedData()
                {
                    LeftText = killer,
                    RightText = m_AIShooter.AIName,
                    Team = bot.AITeam
                };
                feed.AddData("gunid", gunID);
                feed.AddData("headshot", ishead);

                KillFeedBase.Instance.SendKillMessageEvent(feed);

                if (bot != null)
                {
                    killerBot = bot;
                }
                else
                {
                    Debug.Log("Bot who kill this bot can't be found");
                }
            }
        }//else, (if other player kill this bot) -> do nothing.

        if (GameConfig.Instance.showDeathIcons && !isOneTeamMode)
        {
            if (m_AIShooter.AITeam == PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam())
            {
                GameObject di = ObjectPoolingBase.Instance.Instantiate("deathicon", transform.position, transform.rotation);
            }
        }

        var mplayer = new GamePlayer(photonView, false, false);
        GameEvents.DispatchRemotePlayerDeath(mplayer);

        //update the bot deaths count.
        AIMananger.Instance.SetBotDeath(m_AIShooter.AIName);

        if (PhotonNetworkWrapper.IsMasterClient)
        {
            //respawn management here
            //only master client called it since the spawn will be sync by PhotonNetwork.Instantiate()
            AIMananger.Instance.OnBotDeath(m_AIShooter, killerBot);

            //Only Master client should send the RPC
            var deathData = UtilityHelper.CreatePhotonHashTable();
            deathData.Add("type", AIRemoteCallType.DestroyBot);
            deathData.Add("direction", direction);
            if (weaponName.Contains("Grenade"))
            {
                deathData.Add("explosion", true);
            }

            //Should buffer this RPC?
            this.photonView.RPC(AIShooterAgent.RPC_NAME, RpcTarget.All, deathData);//callback is in AIShooterAgent.cs -> DestroyBot(...)
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DestroyEntity()
    {
        var deathData = UtilityHelper.CreatePhotonHashTable();
        deathData.Add("type", AIRemoteCallType.DestroyBot);
        deathData.Add("instant", true);
        this.photonView.RPC(AIShooterAgent.RPC_NAME, RpcTarget.AllBuffered, deathData);//callback is in AIShooterAgent.cs
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DoRepetingDamage(RepetingDamageInfo info)
    {
        repetingDamageInfo = info;
        InvokeRepeating(nameof(MakeDamageRepeting), 0, info.Rate);
    }

    /// <summary>
    /// 
    /// </summary>
    void MakeDamageRepeting()
    {
        if (repetingDamageInfo == null)
        {
            CancelRepetingDamage();
            return;
        }

        var damageinfo = repetingDamageInfo.DamageData;
        if (damageinfo == null)
        {
            damageinfo = new DamageData();
            damageinfo.Direction = Vector3.zero;
            damageinfo.Cause = DamageCause.Map;
        }
        damageinfo.Damage = repetingDamageInfo.Damage;
        DoDamage(damageinfo.Damage, "[Burn]", damageinfo.Direction, GameController.LocalPlayerViewID, false, PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam(), false);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void CancelRepetingDamage()
    {
        CancelInvoke(nameof(MakeDamageRepeting));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="healthToAdd"></param>
    public override void SetHealth(int healthToAdd, bool overrideHealth)
    {
        if (overrideHealth) Health = healthToAdd;
        else Health += healthToAdd;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool Suicide()
    {
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHealth() => Health;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetMaxHealth() => 100;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
        return m_AIShooter.isDeath;
    }

    [PunRPC]
    void RpcSyncHealth(int _health)
    {
        Health = _health;
    }
}