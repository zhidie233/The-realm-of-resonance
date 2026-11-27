using System.Collections.Generic;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Actors;
using Resonance.Multiplayer;

namespace Resonance.Anomaly
{
    public class GravityManager : EnvironmentEffectManager
    {
        private PlayerLocomotionController _localPlayerController;
        private readonly List<GravityFacility> _gravityFacilities = new List<GravityFacility>();

        protected override void OnInitialize()
        {
            FindAllGravityFacilities();
            BindLocalPlayer();
            EventBus.Subscribe<LocalPlayerSpawnedEvent>(HandleLocalPlayerSpawned);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<LocalPlayerSpawnedEvent>(HandleLocalPlayerSpawned);
            if (_localPlayerController != null)
            {
                _localPlayerController.ClearJumpSpeedOverride();
            }
        }

        protected override void OnMatchReset()
        {
            if (_localPlayerController != null)
            {
                _localPlayerController.ClearJumpSpeedOverride();
            }
            UpdateFacilityTransform();
        }

        private void HandleLocalPlayerSpawned(LocalPlayerSpawnedEvent e)
        {
            BindLocalPlayer();
            UpdateEffect();
        }

        private void BindLocalPlayer()
        {
            if (GameController.Instance == null || GameController.Instance.LocalPlayer == null) return;
            _localPlayerController = GameController.Instance.LocalPlayer.GetComponent<PlayerLocomotionController>();
        }

        private void FindAllGravityFacilities()
        {
            _gravityFacilities.Clear();
            _gravityFacilities.AddRange(FindObjectsOfType<GravityFacility>(true));
        }

        public override void UpdateEffect()
        {
            if (_localPlayerController != null)
            {
                float gravityMultiplier = ProjectConfigRoot.Instance.gravityConfig.jumpForce.Evaluate(currentResonance);
                _localPlayerController.SetJumpSpeedOverride(gravityMultiplier);
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
    }
}
