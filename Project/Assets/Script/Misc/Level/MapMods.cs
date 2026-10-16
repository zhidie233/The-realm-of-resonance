﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMods : MonoBehaviour
{
    [GameFwkToogle] public bool infinityAmmo = false;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        GameEvents.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        GameEvents.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerSpawn()
    {
        var p = GameCore.LocalPlayerReferences;

        if (infinityAmmo) p.gunManager.SetInfinityAmmoToAllEquippeds(true);
    }
}