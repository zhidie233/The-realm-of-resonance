using System;
using Photon.Pun;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class MeteoriteFacility : ResonanceFacility
{
    private MeteoriteConfig config => GameConfig.Instance.meteoriteConfig;
    private BoxCollider dropArea;
    private bool isOnCooldown = false;
    private const float COOLDOWN_TIME = 1f;

    private void Start()
    {
        GameObject dropAreaObject = GameObject.FindGameObjectWithTag("DropArea");
        if (dropAreaObject != null) dropArea = dropAreaObject.GetComponent<BoxCollider>();
    }

    public override void ReceiveDamage(DamageInfo damageData)
    {
        if (damageData == null || damageData.GameActor == null) return;

        // 陨石的目标选择与冷却只由房主判定，其他端把请求转给房主
        if (NetworkWrapper.IsMasterClient)
        {
            TryTriggerMeteorite(damageData.GameActor, damageData.ActorViewID);
            return;
        }

        PhotonView view = GetComponent<PhotonView>();
        if (view == null || !NetworkWrapper.InRoom) return;
        view.RPC(nameof(RPC_RequestMeteorite), RpcTarget.MasterClient,
            damageData.GameActor.Name, damageData.ActorViewID);
    }

    [PunRPC]
    private void RPC_RequestMeteorite(string shooterName, int shooterViewID)
    {
        GamePlayer shooter = GameController.Instance.GetGamePlayer(shooterName);
        if (shooter == null) return;

        TryTriggerMeteorite(shooter, shooterViewID);
    }

    private void TryTriggerMeteorite(GamePlayer shooter, int shooterViewID)
    {
        if (isOnCooldown) return;

        // 检查触发概率
        if (Random.Range(0f, 1f) > config.triggerProbability) return;

        // 获取敌方队伍
        Team enemyTeam = GetEnemyTeam(shooter.Team);
        if (enemyTeam == Team.None) return;

        // 随机选择一个敌方目标
        GamePlayer target = GetRandomEnemyPlayer(enemyTeam);
        if (target == null) return;

        // 开始冷却
        isOnCooldown = true;
        StartCoroutine(CooldownRoutine());

        // 通知所有客户端生成陨石
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(RPC_TriggerMeteorite), RpcTarget.All,
            shooter.Name,
            target.Name,
            shooterViewID);
    }
    
    [PunRPC]
    private void RPC_TriggerMeteorite(string shooterName, string targetName, int shooterViewID)
    {
        GamePlayer shooter = GameController.Instance.GetGamePlayer(shooterName);
        GamePlayer target = GameController.Instance.GetGamePlayer(targetName);
        if (shooter != null && target != null)
        {
            StartCoroutine(SpawnMeteorite(shooter, target, shooterViewID));
        }
    }
    
    private IEnumerator SpawnMeteorite(GamePlayer shooter, GamePlayer target, int shooterViewID)
    {
        // 显示射击者标记
        if (shooter.isRealPlayer && shooter.Name == NetworkWrapper.NickName)
        {
            ShowShooterMarker(shooter);
        }
        
        // 等待倒计时
        yield return new WaitForSeconds(config.meteoriteCountdown);
        
        if (target == null || target.Actor == null) yield break;
        
        // 计算伤害（基于共鸣值）
        float resonance = ResonanceManager.Instance.GetCurrentResonance();
        float damageMultiplier = config.damageCurve.Evaluate(resonance);
        int finalDamage = Mathf.RoundToInt(config.baseDamage * damageMultiplier);
        
        // 在目标位置上方生成陨石
        Vector3 spawnPosition = GetRandomPositionInDropArea();
        SpawnMeteoriteProjectile(spawnPosition, target.Actor.position, finalDamage, shooterViewID, shooter);
    }
    
    private void SpawnMeteoriteProjectile(Vector3 spawnPos, Vector3 targetPos, int damage, int shooterViewID, GamePlayer shooter)
    {
        // 使用现有的榴弹发射器逻辑，但修改为陨石行为
        GameObject meteorite = Instantiate(config.meteoritePrefab, spawnPos, Quaternion.identity);
        var projectile = meteorite.GetComponent<MeteoriteProjectile>();
        // 创建子弹数据
        ShotData bulletData = new ShotData
        {
            Damage = damage,
            GameActor = shooter,
            ActorViewID = shooterViewID,
            Speed = config.meteoriteFallSpeed,
            Position = spawnPos,
            // 伤害与共鸣只由房主结算，其余客户端只做表现
            isNetwork = !NetworkWrapper.IsMasterClient
        };

        // 先朝向目标再初始化，避免初速度沿预制体默认朝向
        meteorite.transform.forward = (targetPos - spawnPos).normalized;
        projectile.InitProjectile(bulletData);
        projectile.SetTarget(targetPos);
    }
    
    private void ShowShooterMarker(GamePlayer shooter)
    {
        if (config.arrowMarkerPrefab == null) return;
        
        GameObject marker = Instantiate(config.arrowMarkerPrefab);
        MeteoriteShooterMarker markerScript = marker.GetComponent<MeteoriteShooterMarker>();
        if (markerScript != null)
        {
            markerScript.Initialize(shooter, config.markerDisplayTime);
        }
    }
    
    private Team GetEnemyTeam(Team shooterTeam)
    {
        return shooterTeam == Team.Team1 ? Team.Team2 : Team.Team1;
    }
    
    private GamePlayer GetRandomEnemyPlayer(Team enemyTeam)
    {
        GamePlayer[] enemies = GameController.Instance.GetGamePlayerInTeam(enemyTeam, true);
        return enemies.Length > 0 ? enemies[Random.Range(0, enemies.Length)] : null;
    }
    
    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(COOLDOWN_TIME);
        isOnCooldown = false;
    }
    
    private Vector3 GetRandomPositionInDropArea()
    {
        if (dropArea == null) return Vector3.zero;
        
        // 计算掉落区域内的随机位置
        Vector3 localRandomPos = new Vector3(
            Random.Range(-dropArea.size.x * 0.5f, dropArea.size.x * 0.5f),
            Random.Range(-dropArea.size.y * 0.5f, dropArea.size.y * 0.5f),
            Random.Range(-dropArea.size.z * 0.5f, dropArea.size.z * 0.5f)
        );
        
        // 转换为世界坐标
        Vector3 worldPos = dropArea.transform.TransformPoint(localRandomPos);
        Vector3 offset=new Vector3(0, 10f, 0);
        return worldPos-offset;
    }
}