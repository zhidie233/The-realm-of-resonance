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
    [DisallowMultipleComponent, CreateAssetMenu(fileName = "Player Class Preset", menuName = "项目/Player Class/Equipment")]
    public class PlayerClassPreset : ScriptableObject
    {
        [FirearmID] public int Primary = 0;
        [FirearmID] public int Secondary = 1;
        [FirearmID] public int Perks = 2;
        [FirearmID] public int Lethal = 3;

        public void FromString(string str)
        {
            string[] split = str.Split('&');
            Primary = int.Parse(split[0]);
            Secondary = int.Parse(split[1]);
            Perks = int.Parse(split[2]);
            Lethal = int.Parse(split[3]);
        }

        public void FromString(string str, int slot)
        {
            string[] presets = str.Split(',');
            string[] split = presets[slot].Split('&');
            Primary = int.Parse(split[0]);
            Secondary = int.Parse(split[1]);
            Perks = int.Parse(split[2]);
            Lethal = int.Parse(split[3]);
        }

        public FirearmData GetPrimaryGunInfo() => ProjectConfigRoot.Instance.GetWeapon(Primary);
        public FirearmData GetSecondaryGunInfo() => ProjectConfigRoot.Instance.GetWeapon(Secondary);
        public FirearmData GetPerksGunInfo() => ProjectConfigRoot.Instance.GetWeapon(Perks);
        public FirearmData GetLethalGunInfo() => ProjectConfigRoot.Instance.GetWeapon(Lethal);
        public override string ToString() => $"{Primary}&{Secondary}&{Perks}&{Lethal}";
    }
}
