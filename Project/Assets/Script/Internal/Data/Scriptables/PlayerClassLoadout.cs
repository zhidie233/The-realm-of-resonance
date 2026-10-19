using UnityEngine;

[DisallowMultipleComponent, CreateAssetMenu(fileName = "Player Class Loadout", menuName = "Game/Player Class/Loadout")]
public class PlayerClassLoadout : ScriptableObject
{
    [FirearmID] public int Primary = 0;
    [FirearmID] public int Secondary = 1;
    [FirearmID] public int Perks = 2;
    [FirearmID] public int Letal = 3;

    public void FromString(string str)
    {
        string[] split = str.Split('&');
        Primary = int.Parse(split[0]);
        Secondary = int.Parse(split[1]);
        Perks = int.Parse(split[2]);
        Letal = int.Parse(split[3]);
    }

    public void FromString(string str, int slot)
    {
        string[] loadouts = str.Split(',');
        string[] split = loadouts[slot].Split('&');
        Primary = int.Parse(split[0]);
        Secondary = int.Parse(split[1]);
        Perks = int.Parse(split[2]);
        Letal = int.Parse(split[3]);
    }

    public FirearmData GetPrimaryGunInfo() => GameConfig.Instance.GetWeapon(Primary);
    public FirearmData GetSecondaryGunInfo() => GameConfig.Instance.GetWeapon(Secondary);
    public FirearmData GetPerksGunInfo() => GameConfig.Instance.GetWeapon(Perks);
    public FirearmData GetLetalGunInfo() => GameConfig.Instance.GetWeapon(Letal);
    public override string ToString() => $"{Primary}&{Secondary}&{Perks}&{Letal}";
}