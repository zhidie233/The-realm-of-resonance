using System.Collections.Generic;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.UI;
namespace Resonance.Anomaly
{

    public class GroundFireManager : EnvironmentEffectManager
    {
        private Transform groundFiresTransform;
        private List<Transform> fireZones = new List<Transform>();

        private List<GameObject> activeFires = new List<GameObject>();
        private bool isActive = false;

        protected override void Start()
        {
            base.Start();
            if (!enabled) return;

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
            if (currentResonance >= ProjectConfigRoot.Instance.groundFireConfig.activationThreshold)
            {
                if (!isActive)
                {
                    ActivateFires();
                    isActive = true;
                }
                UpdateFireIntensity();
            }
            else if (isActive)
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

        private void UpdateFireIntensity()
        {
            float damage = ProjectConfigRoot.Instance.groundFireConfig.fireDamageCurve.Evaluate(currentResonance);
            foreach (var fire in activeFires)
            {
                var fireComponent = fire.GetComponent<DamageArea>();
                if (fireComponent != null)
                    fireComponent.Damage = (int)damage;

                fire.transform.localScale = new Vector3(1, (1f + currentResonance * 0.01f) * ProjectConfigRoot.Instance.groundFireConfig.effectScaleMultiplier, 1);
            }
        }

        private void DeactivateFires()
        {
            foreach (var fire in activeFires)
            {
                Destroy(fire);
            }
            activeFires.Clear();
        }
    }
}
