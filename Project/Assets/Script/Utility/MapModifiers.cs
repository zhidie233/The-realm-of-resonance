using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapModifiers : MonoBehaviour
{
    [GameToggle] public bool infinityAmmo = false;

    private void OnEnable()
    {
        GameEvents.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    private void OnDisable()
    {
        GameEvents.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }

    void OnLocalPlayerSpawn()
    {
        var p = GameCore.LocalPlayerReferences;

        if (infinityAmmo) p.gunManager.SetInfinityAmmoToAllEquippeds(true);
    }
}