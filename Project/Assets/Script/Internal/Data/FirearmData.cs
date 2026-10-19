using System;
using UnityEngine;
using Game.Internal.Structures;
using GameEditor;

[Serializable]
public class FirearmData
{
    [Header("Info")]
    public string Name;
    public FirearmType Type = FirearmType.Machinegun;
    [GameToggle] public bool Active = true;

    [Header("Settings")]
    [Range(1, 100)] public int Damage;
    [Range(0.01f, 2f)] public float FireRate = 0.1f;
    [Range(0.5f, 10)] public float ReloadTime = 2.5f;
    [Range(0, 1000)] public int Range;
    [Range(0.01f, 5)] public int Accuracy;
    [Range(0, 4)] public float Weight;
    public GameItemUnlockability Unlockability;

    [Header("References")]
    public FirearmPickUpBase PickUpPrefab;
    [SpritePreview(30, true)] public Sprite GunIcon;

    /// <summary>
    /// 此武器是否显示在游戏菜单中（配装、解锁等）
    /// </summary>
    /// <returns></returns>
    public bool CanShowWeapon()
    {
        return Active && Unlockability.UnlockMethod != GameItemUnlockability.UnlockabilityMethod.Hidden;
    }
}