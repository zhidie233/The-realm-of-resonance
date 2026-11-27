using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Combat;
namespace Resonance.Core
{

    [CreateAssetMenu(fileName = "WeaponCatalog", menuName = "项目/Config/Weapon Catalog")]
    public class WeaponCatalogConfig : ScriptableObject
    {
        [NonReorderable]
        public List<FirearmData> WeaponTable = new List<FirearmData>();
        [ScriptableDrawer] public WeaponSlotGuide weaponSlotGuide;

        public FirearmData GetWeapon(int id)
        {
            if (id < 0 || id > WeaponTable.Count - 1) return WeaponTable[0];
            return WeaponTable[id];
        }

        public int GetWeaponID(string gunName)
        {
            if (WeaponTable.Exists(x => x.Name == gunName))
                return WeaponTable.FindIndex(x => x.Name == gunName);
            return -1;
        }

        public string[] WeaponStringList() => WeaponTable.Select(x => x.Name).ToArray();
    }
}
