using System.Collections.Generic;
using UnityEngine;
using Resonance.Domain.Anomaly;
using Resonance.Core;
using Resonance.World;

namespace Resonance.Anomaly
{
    public class GroundFireManager : EnvironmentEffectManager
    {
        private Transform groundFiresTransform;
        private readonly List<Transform> fireZones = new List<Transform>();
        private readonly List<GameObject> activeFires = new List<GameObject>();
        private bool isActive = false;

        protected override void OnInitialize()
        {
            GameObject groundFireRoot = GameObject.FindGameObjectWithTag("GroundFire");
            if (groundFireRoot == null)
            {
                enabled = false;
                return;
            }

            groundFiresTransform = groundFireRoot.transform;
            fireZones.AddRange(groundFiresTransform.GetComponentsInChildren<Transform>());
            fireZones.Remove(groundFiresTransform);
        }

        public override void UpdateEffect()
        {
            var cfg = ProjectConfigRoot.Instance.groundFireConfig;
            var decision = GroundFireRule.Evaluate(currentResonance, cfg.activationThreshold, cfg.effectScaleMultiplier, isActive);

            if (decision.ShouldActivate) ActivateFires();
            if (decision.ShouldUpdateIntensity) UpdateFireIntensity(decision.VerticalScale);
            else if (decision.ShouldDeactivate) DeactivateFires();

            isActive = decision.IsActive;
        }

        protected override void OnMatchReset()
        {
            if (isActive)
            {
                DeactivateFires();
                isActive = false;
            }
        }

        private void ActivateFires()
        {
            foreach (var zone in fireZones)
            {
                var fire = Instantiate(ProjectConfigRoot.Instance.groundFireConfig.fireEffectPrefab, zone.position, Quaternion.identity, zone);
                activeFires.Add(fire);
            }
        }

        private void UpdateFireIntensity(float verticalScale)
        {
            float damage = ProjectConfigRoot.Instance.groundFireConfig.fireDamageCurve.Evaluate(currentResonance);
            foreach (var fire in activeFires)
            {
                var fireComponent = fire.GetComponent<DamageArea>();
                if (fireComponent != null)
                    fireComponent.Damage = (int)damage;

                fire.transform.localScale = new Vector3(1f, verticalScale, 1f);
            }
        }

        private void DeactivateFires()
        {
            foreach (var fire in activeFires) Destroy(fire);
            activeFires.Clear();
        }
    }
}
