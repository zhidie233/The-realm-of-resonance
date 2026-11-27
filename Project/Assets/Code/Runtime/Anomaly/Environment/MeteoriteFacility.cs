using System.Collections;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Multiplayer;
using Resonance.World;
using Random = UnityEngine.Random;

namespace Resonance.Anomaly
{
    public class MeteoriteFacility : ResonanceFacility
    {
        private const string ActionRequest = "req";
        private const string ActionTrigger = "fire";
        private const float COOLDOWN_TIME = 1f;

        private MeteoriteConfig config => ProjectConfigRoot.Instance.meteoriteConfig;
        private BoxCollider dropArea;
        private bool isOnCooldown = false;

        private void Start()
        {
            dropArea = WorldDropZone.FindDropArea();
        }

        private void OnEnable()
        {
            NetworkWrapper.AddNetworkCallback(NetProtocol.MeteoriteTriggerEvent, HandleMeteoriteNetworkEvent);
        }

        private void OnDisable()
        {
            NetworkWrapper.RemoveNetworkCallback(HandleMeteoriteNetworkEvent);
        }

        public override void ReceiveDamage(DamageInfo damageData)
        {
            if (damageData == null || damageData.GameActor == null) return;
            base.ReceiveDamage(damageData);

            if (NetworkWrapper.IsMasterClient)
            {
                TryTriggerMeteorite(damageData.GameActor, damageData.ActorViewID);
                return;
            }

            if (!NetworkWrapper.InRoom || NetworkWrapper.Instance == null || NetworkWrapper.MasterClient == null) return;
            var payload = new Hashtable
            {
                ["act"] = ActionRequest,
                ["facility"] = gameObject.name,
                ["shooter"] = damageData.GameActor.Name,
                ["viewId"] = damageData.ActorViewID
            };
            NetworkWrapper.Instance.SendDataOverNetworkToPlayer(NetProtocol.MeteoriteTriggerEvent, payload, NetworkWrapper.MasterClient);
        }

        private void HandleMeteoriteNetworkEvent(Hashtable data)
        {
            if (data == null || !data.ContainsKey("act") || !data.ContainsKey("facility")) return;
            if ((string)data["facility"] != gameObject.name) return;

            string action = (string)data["act"];
            if (action == ActionRequest)
            {
                if (!NetworkWrapper.IsMasterClient) return;
                GamePlayer shooter = GameController.Instance.GetGamePlayer((string)data["shooter"]);
                if (shooter != null)
                {
                    TryTriggerMeteorite(shooter, (int)data["viewId"]);
                }
            }
            else if (action == ActionTrigger)
            {
                GamePlayer shooter = GameController.Instance.GetGamePlayer((string)data["shooter"]);
                GamePlayer target = GameController.Instance.GetGamePlayer((string)data["target"]);
                if (shooter != null && target != null)
                {
                    StartCoroutine(SpawnMeteorite(shooter, target, (int)data["viewId"]));
                }
            }
        }

        private void TryTriggerMeteorite(GamePlayer shooter, int shooterViewID)
        {
            if (isOnCooldown) return;
            if (Random.Range(0f, 1f) > config.triggerProbability) return;

            Team enemyTeam = GetEnemyTeam(shooter.Team);
            if (enemyTeam == Team.None) return;

            GamePlayer target = GetRandomEnemyPlayer(enemyTeam);
            if (target == null) return;

            isOnCooldown = true;
            StartCoroutine(CooldownRoutine());

            var payload = new Hashtable
            {
                ["act"] = ActionTrigger,
                ["facility"] = gameObject.name,
                ["shooter"] = shooter.Name,
                ["target"] = target.Name,
                ["viewId"] = shooterViewID
            };
            NetworkWrapper.Instance.SendDataOverNetwork(NetProtocol.MeteoriteTriggerEvent, payload);
        }

        private IEnumerator SpawnMeteorite(GamePlayer shooter, GamePlayer target, int shooterViewID)
        {
            if (shooter.isRealPlayer && shooter.Name == NetworkWrapper.NickName)
            {
                ShowShooterMarker(shooter);
            }

            yield return new WaitForSeconds(config.meteoriteCountdown);

            if (target == null || target.Actor == null) yield break;

            float resonance = ResonanceManager.Instance.GetCurrentResonance();
            float damageMultiplier = config.damageCurve.Evaluate(resonance);
            int finalDamage = Mathf.RoundToInt(config.baseDamage * damageMultiplier);

            Vector3 spawnPosition = WorldDropZone.SampleWorldPoint(dropArea, 10f);
            SpawnMeteoriteProjectile(spawnPosition, target.Actor.position, finalDamage, shooterViewID, shooter);
        }

        private void SpawnMeteoriteProjectile(Vector3 spawnPos, Vector3 targetPos, int damage, int shooterViewID, GamePlayer shooter)
        {
            GameObject meteorite = Instantiate(config.meteoritePrefab, spawnPos, Quaternion.identity);
            var projectile = meteorite.GetComponent<MeteoriteProjectile>();
            ShotData bulletData = new ShotData
            {
                Damage = damage,
                GameActor = shooter,
                ActorViewID = shooterViewID,
                Speed = config.meteoriteFallSpeed,
                Position = spawnPos,
                isNetwork = !NetworkWrapper.IsMasterClient
            };

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

        private Team GetEnemyTeam(Team shooterTeam) => shooterTeam == Team.Team1 ? Team.Team2 : Team.Team1;

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
    }
}
