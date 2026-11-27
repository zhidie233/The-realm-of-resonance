using TMPro;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
namespace Resonance.UI
{

    public class HeldWeaponUI : MonoBehaviour
    {
        private static HeldWeaponUI _instance;
        public static HeldWeaponUI Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<HeldWeaponUI>(); }
                if (_instance == null) { _instance = UIReferences.Instance.GetComponentInChildren<HeldWeaponUI>(true); }
                return _instance;
            }
        }

        [SerializeField] private TextMeshProUGUI AmmoText;
        [SerializeField] private TextMeshProUGUI ClipText;
        [SerializeField] private TextMeshProUGUI FireTypeText;
        public Gradient AmmoTextColorGradient;

        /// 更新本地玩家界面中的弹药信息
        public void SetAmmoOf(FirearmController gun)
        {
            int bullets = gun.bulletsLeft;
            int clips = gun.RemainingClips;
            float per = (float)bullets / (float)gun.bulletsPerClip;
            Color c = AmmoTextColorGradient.Evaluate(per);

            if (gun.Info.Type != FirearmType.Knife)
            {
                AmmoText.text = bullets.ToString();
                if (gun.HasInfiniteAmmo)
                    ClipText.text = "∞";
                else
                    ClipText.text = ClipText.text = clips.ToString("F0");
                AmmoText.color = c;
                ClipText.color = c;
            }
            else
            {
                AmmoText.text = "--";
                ClipText.text = ClipText.text = "--";
                AmmoText.color = Color.white;
                ClipText.color = Color.white;
            }
        }

        /// 更新本地玩家界面中的开火模式文本。
        public void SetFireType(FirearmController.FireType fireType)
        {
            if (FireTypeText == null) return;

            string fireName = "--";
            switch (fireType)
            {
                case FirearmController.FireType.Auto:   fireName = GameTexts.FireTypeAuto;   break;
                case FirearmController.FireType.Semi:   fireName = GameTexts.FireTypeSemi;   break;
                case FirearmController.FireType.Single: fireName = GameTexts.FireTypeSingle; break;
            }
            FireTypeText.text = fireName;
        }
    }
}
