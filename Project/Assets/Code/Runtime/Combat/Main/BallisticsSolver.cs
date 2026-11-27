using UnityEngine;
using Resonance.Foundation;
using Resonance.Domain.Combat;
using Resonance.Core;
using Resonance.Multiplayer;

namespace Resonance.Combat
{
    /// <summary>
    /// Computes shot spawn transforms and populates ShotData packets for projectiles.
    /// </summary>
    public static class BallisticsSolver
    {
        public static FirearmController.ShotInstanceData ComputeShotInstanceData(
            Transform weaponParent,
            Camera playerCamera,
            Transform muzzlePoint,
            LayerMask hittableLayers,
            bool isAiming,
            FirearmType weaponType)
        {
            var data = new FirearmController.ShotInstanceData();
            data.Rotation = weaponParent.rotation;

            Vector3 cp = playerCamera.transform.position;
            Vector3 mp = muzzlePoint.position;
            data.ProjectedHitPoint = cp + (playerCamera.transform.forward * 100f);

            data.Position = Physics.Linecast(cp, mp, hittableLayers) ? cp : mp;

            if (Physics.Raycast(data.Position, playerCamera.transform.forward, out RaycastHit hit, 5f, hittableLayers, QueryTriggerInteraction.Ignore))
            {
                data.ProjectedHitPoint = hit.point;
                data.Position = cp;
            }
            else if (isAiming)
            {
                if (weaponType != FirearmType.Sniper)
                {
                    data.Rotation = Quaternion.LookRotation(data.ProjectedHitPoint - mp);
                    if (Physics.Raycast(cp, playerCamera.transform.forward, out hit, 500f, hittableLayers, QueryTriggerInteraction.Ignore))
                    {
                        data.ProjectedHitPoint = hit.point;
                        data.Rotation = Quaternion.LookRotation(data.ProjectedHitPoint - mp);
                    }
                }
                else
                {
                    data.Position = cp;
                }
            }

            return data;
        }

        public static void PopulateBulletData(ShotData target, FirearmController weapon, SpreadRecoilModel spreadModel)
        {
            target.Damage = weapon.Damage;
            target.ImpactForce = weapon.impactForce;

            spreadModel.ComputeInaccuracy(weapon.WeaponType == FirearmType.Sniper, weapon.isAiming, out float spread, out float maxSpread);
            target.SetInaccuracy(spread, maxSpread);

            target.Speed = weapon.BulletSpeed;
            target.WeaponName = weapon.Info.Name;
            target.Position = weapon.PlayerReferences.transform.localPosition;
            target.WeaponID = weapon.WeaponID;
            target.isNetwork = false;
            target.DropFactor = weapon.bulletDropFactor;
            target.Range = weapon.Range;
            target.ActorViewID = GameController.LocalPlayerViewID;
            target.GameActor = GameController.Instance.LocalActor;
            target.IsLocalPlayer = true;
        }
    }
}
