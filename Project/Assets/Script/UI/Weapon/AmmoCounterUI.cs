using UnityEngine;
using TMPro;

namespace Game.Runtime.UI
{
    /// <summary>
    /// 默认游戏弹药指示器处理器
    /// 此脚本用于显示文本，指示当前装备武器的弹药状态
    /// 让玩家知道需要换弹、弹药不足还是已打空。
    /// 此脚本无引用依赖，不想用或想换成自定义实现时，直接从 UI 实例上移除即可。
    /// </summary>
    public class AmmoCounterUI : MonoBehaviour
    {
        [GameToggle] public bool forceUpperCase = true;
        public TextMeshProUGUI indicatorText;

        private readonly Color m_warninColor = Color.yellow;
        private readonly Color m_alertColor = Color.red;

        private void OnEnable()
        {
            SetActive(false);
            GameEvents.onLocalPlayerAmmoUpdate += OnLocalAmmoChange;
            GameEvents.onChangeWeapon += onWeaponChanged;
        }

        private void OnDisable()
        {
            GameEvents.onLocalPlayerAmmoUpdate -= OnLocalAmmoChange;
            GameEvents.onChangeWeapon -= onWeaponChanged;
        }

        void OnLocalAmmoChange(int ammoCount)
        {
            var player = GameCore.LocalPlayerReferences;
            if (player == null) return;

            var weapon = player.gunManager.GetCurrentWeapon();
            if (weapon == null) return;

            var type = weapon.Info.Type;
            if (type == FirearmType.Knife) { SetActive(false); return; }

            int bullets = weapon.bulletsLeft;
            if (bullets <= 0)
            {
                SetText(GameTexts.OutOfAmmo);
                SetColor(m_alertColor);
                return;
            }

            //这些武器类型只需在没弹药时提示
            if (type == FirearmType.Grenade || type == FirearmType.Launcher)
            {
                SetActive(false);
                return;
            }

            int max = weapon.bulletsPerClip;
            //弹匣容量太小时不必提示
            if (max <= 4) { SetActive(false); return; }

            int third = Mathf.FloorToInt(max / 3);
 

            //武器状态正常。
            if (bullets > third) { SetActive(false); return; }

            int five = Mathf.FloorToInt(max / 5);
            //武器剩余弹药仅为弹匣容量的 1/5
            if (bullets <= third && bullets > five)
            {
                SetText(GameTexts.Reload.ToUpper());
                SetColor(Color.white);
                return;
            }

            //只剩一种可能 = 剩余弹药为最大弹药的 1/5 = 弹药不足
            SetText(GameTexts.LowAmmo);
            SetColor(m_warninColor );
        }

        void onWeaponChanged(int newWeapon)
        {
            OnLocalAmmoChange(0);
        }

        private void SetText(string text)
        {
            if (indicatorText == null) return;
            if (forceUpperCase) text = text.ToUpper();
            indicatorText.text = text;
            SetActive(true);
        }

        private void SetColor(Color color)
        {
            if (indicatorText == null) return;
            indicatorText.color = color;
        }

        public void SetActive(bool active)
        {
            if (indicatorText == null) return;

            indicatorText.gameObject.SetActive(active);
        }

    }
}