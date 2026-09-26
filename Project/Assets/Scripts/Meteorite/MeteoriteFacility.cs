using System;
using Photon.Pun;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class MeteoriteFacility : ResonanceFacility
{
    [Header("陨石配置")]
    public MeteoriteConfig meteoriteConfig;
    private BoxCollider dropArea;
    private bool isOnCooldown = false;
    private float lastTriggerTime = 0f;
    private const float COOLDOWN_TIME = 1f;

    private void Start()
    {
        dropArea=GameObject.FindGameObjectWithTag("DropArea").GetComponent<BoxCollider>();
    }

    public override void ReceiveDamage(DamageData damageData)
    {
        if (isOnCooldown) return;
        
        // 检查触发概率
        if (Random.Range(0f, 1f) > meteoriteConfig.triggerProbability) return;
        
        // 只有MasterClient处理陨石生成逻辑
        if (!PhotonNetwork.IsMasterClient) return;
        
        TriggerMeteorite(damageData);
    }
    
    private void TriggerMeteorite(DamageData damageData)
    {
        // 获取敌方队伍
        Team enemyTeam = GetEnemyTeam(damageData.GFWKActor.Team);
        if (enemyTeam == Team.None) return;
        
        // 随机选择一个敌方目标
        GFWKPlayer target = GetRandomEnemyPlayer(enemyTeam);
        if (target == null) return;
        
        // 开始冷却
        isOnCooldown = true;
        lastTriggerTime = Time.time;
        StartCoroutine(CooldownRoutine());
        
        // 通知所有客户端生成陨石
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC("RPC_TriggerMeteorite", RpcTarget.All, 
            damageData.GFWKActor.Name, 
            target.Name,
            damageData.ActorViewID);
    }
    
    [PunRPC]
    private void RPC_TriggerMeteorite(string shooterName, string targetName, int shooterViewID)
    {
        GFWKPlayer shooter = bl_GameManager.Instance.GetGFWKPlayer(shooterName);
        GFWKPlayer target = bl_GameManager.Instance.GetGFWKPlayer(targetName);
        if (shooter != null && target != null)
        {
            StartCoroutine(SpawnMeteorite(shooter, target, shooterViewID));
        }
    }
    
    private IEnumerator SpawnMeteorite(GFWKPlayer shooter, GFWKPlayer target, int shooterViewID)
    {
        // 显示射击者标记
        if (shooter.isRealPlayer && shooter.Name == PhotonNetwork.NickName)
        {
            ShowShooterMarker(shooter);
        }
        
        // 等待倒计时
        yield return new WaitForSeconds(meteoriteConfig.meteoriteCountdown);
        
        if (target == null || target.Actor == null) yield break;
        
        // 计算伤害（基于共鸣值）
        float resonance = ResonanceManager.Instance.GetCurrentResonance();
        float damageMultiplier = meteoriteConfig.damageCurve.Evaluate(resonance);
        int finalDamage = Mathf.RoundToInt(meteoriteConfig.baseDamage * damageMultiplier);
        
        // 在目标位置上方生成陨石
        Vector3 spawnPosition = GetRandomPositionInDropArea();
        SpawnMeteoriteProjectile(spawnPosition, target.Actor.position, finalDamage, shooterViewID, shooter);
    }
    
    private void SpawnMeteoriteProjectile(Vector3 spawnPos, Vector3 targetPos, int damage, int shooterViewID, GFWKPlayer shooter)
    {
        // 使用现有的榴弹发射器逻辑，但修改为陨石行为
        GameObject meteorite = Instantiate(meteoriteConfig.meteoritePrefab, spawnPos, Quaternion.identity);
        var projectile = meteorite.GetComponent<MeteoriteProjectile>();
        // 创建子弹数据
        BulletData bulletData = new BulletData
        {
            Damage = damage,
            GFWKActor = shooter,
            ActorViewID = shooterViewID,
            Speed = meteoriteConfig.meteoriteFallSpeed,
            Position = spawnPos,
            isNetwork = false
        };
        
        projectile.InitProjectile(bulletData);
        projectile.SetTarget(targetPos);
        
        // 设置陨石朝向目标
        Vector3 direction = (targetPos - spawnPos).normalized;
        meteorite.transform.forward = direction;
    }
    
    private void ShowShooterMarker(GFWKPlayer shooter)
    {
        if (meteoriteConfig.arrowMarkerPrefab == null) return;
        
        GameObject marker = Instantiate(meteoriteConfig.arrowMarkerPrefab);
        MeteoriteShooterMarker markerScript = marker.GetComponent<MeteoriteShooterMarker>();
        if (markerScript != null)
        {
            markerScript.Initialize(shooter, meteoriteConfig.markerDisplayTime);
        }
    }
    
    private Team GetEnemyTeam(Team shooterTeam)
    {
        return shooterTeam == Team.Team1 ? Team.Team2 : Team.Team1;
    }
    
    private GFWKPlayer GetRandomEnemyPlayer(Team enemyTeam)
    {
        GFWKPlayer[] enemies = bl_GameManager.Instance.GetGFWKPlayerInTeam(enemyTeam, true);
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
    
    private void Update()
    {
        // 备用冷却检查
        if (isOnCooldown && Time.time - lastTriggerTime >= COOLDOWN_TIME)
        {
            isOnCooldown = false;
        }
    }
}