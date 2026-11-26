using UnityEngine;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{

    public class LevelAdjustments : MonoBehaviour
    {
        [GameToggle] public bool infinityAmmo = false;

        private void OnEnable()
        {
            EventBus.Subscribe<LocalPlayerSpawnedEvent>(OnEvt_onLocalPlayerSpawn_OnLocalPlayerSpawn);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LocalPlayerSpawnedEvent>(OnEvt_onLocalPlayerSpawn_OnLocalPlayerSpawn);
        }

        void OnLocalPlayerSpawn()
        {
            var p = RuntimeServices.LocalPlayerReferences;

            if (infinityAmmo) p.gunManager.SetInfinityAmmoToAllEquipped(true);
        }

        private void OnEvt_onLocalPlayerSpawn_OnLocalPlayerSpawn(LocalPlayerSpawnedEvent e) => OnLocalPlayerSpawn();
    }
}
