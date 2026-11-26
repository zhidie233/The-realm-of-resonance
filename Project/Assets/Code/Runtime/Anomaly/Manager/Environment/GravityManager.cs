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

    public class GravityManager : EnvironmentEffectManager
    {
        private PlayerLocomotionController _localPlayerController;
        private List<GravityFacility> _gravityFacilities = new List<GravityFacility>();

        protected override void Start()
        {
            base.Start();
            if (!enabled) return;

            FindAllGravityFacilities();
            BindLocalPlayer();
            UpdateEffect();

            EventBus.Subscribe<LocalPlayerSpawnedEvent>(OnEvt_onLocalPlayerSpawn_OnLocalPlayerSpawn);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventBus.Unsubscribe<LocalPlayerSpawnedEvent>(OnEvt_onLocalPlayerSpawn_OnLocalPlayerSpawn);
        }

        void OnLocalPlayerSpawn()
        {
            BindLocalPlayer();
            UpdateEffect();
        }

        void BindLocalPlayer()
        {
            if (GameController.Instance == null || GameController.Instance.LocalPlayer == null) return;

            _localPlayerController = GameController.Instance.LocalPlayer.GetComponent<PlayerLocomotionController>();
        }

        private void FindAllGravityFacilities()
        {
            _gravityFacilities.Clear();
            _gravityFacilities.AddRange(Object.FindObjectsOfType<GravityFacility>(true));
        }

        public override void UpdateEffect()
        {
            if (_localPlayerController != null)
            {
                float gravityMultiplier = ProjectConfigRoot.Instance.gravityConfig.jumpForce.Evaluate(currentResonance);
                _localPlayerController.jumpSpeed = gravityMultiplier;
            }

            UpdateFacilityTransform();
        }

        private void UpdateFacilityTransform()
        {
            float maxResonance = ProjectConfigRoot.Instance.gravityConfig.GetMaxResonanceValue();

            foreach (GravityFacility facility in _gravityFacilities)
            {
                if (facility != null)
                    facility.UpdateTransformBasedOnResonance(currentResonance, maxResonance);
            }
        }

        private void OnEvt_onLocalPlayerSpawn_OnLocalPlayerSpawn(LocalPlayerSpawnedEvent e) => OnLocalPlayerSpawn();
    }
}
