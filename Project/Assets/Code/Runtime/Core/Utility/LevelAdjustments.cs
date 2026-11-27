using UnityEngine;
using Resonance.Foundation;
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

            if (infinityAmmo) p.gunManager.SetInfiniteAmmoToAllEquipped(true);
        }

        private void OnEvt_onLocalPlayerSpawn_OnLocalPlayerSpawn(LocalPlayerSpawnedEvent e) => OnLocalPlayerSpawn();
    }
}
