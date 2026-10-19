using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Game.Internal.Structures;
using TMPro;

namespace Game.Runtime.UI.Bindings
{
    public class KillReportBinding : KillReportBindingBase
    {
        [SerializeField] private TextMeshProUGUI KillerText = null;
        [SerializeField] private TextMeshProUGUI KilledText = null;
        [SerializeField] private TextMeshProUGUI WeaponText = null;
        public Image WeaponIconImg;
        [SerializeField] private Image KillTypeImage = null;
        private CanvasGroup Alpha;

        public override void Init(KillReport feed)
        {
            if (Alpha == null) Alpha = GetComponent<CanvasGroup>();
            StopAllCoroutines();
            SetActiveAll(true);
            Alpha.alpha = 1;
            switch (feed.messageType)
            {
                case KillReportMessageType.WeaponKillEvent:
                    OnKillMessage(feed);
                    break;
                case KillReportMessageType.Message:
                    OnMessage(feed);
                    break;
                case KillReportMessageType.TeamHighlightMessage:
                    OnTeamHighight(feed);
                    break;
            }
            gameObject.SetActive(true);
            StartCoroutine(Hide(10));
        }

        void OnKillMessage(KillReport info)
        {
            KillerText.text = info.Killer;
            KilledText.text = info.Killed;
            KillerText.color = isLocalPlayerName(info.Killer) ? GameConfig.Instance.highLightColor : info.KillerTeam.GetTeamColor();
            KilledText.color = isLocalPlayerName(info.Killed) ? GameConfig.Instance.highLightColor : GetOppositeTeam(info.KillerTeam).GetTeamColor();
            if (GameConfig.Instance.killFeedWeaponShowMode == KillReportWeaponShowMode.WeaponName)
            {
                WeaponIconImg.gameObject.SetActive(false);
                WeaponText.text = info.Message;
            }
            else
            {
                WeaponText.gameObject.SetActive(false);
                Sprite icon = null;
                if (info.GunID >= 0)
                {
                    icon = GameConfig.Instance.GetWeapon(info.GunID).GunIcon;
                }
                else
                {
                    if (!string.IsNullOrEmpty(info.Message))
                        icon = KillReportEntry.Instance.GetCustomIcon(info.Message);

                    if (icon == null)
                    {
                        int normalizedID = Mathf.Abs(info.GunID + 1);
                        if (normalizedID <= KillReportEntry.Instance.customIcons.Count - 1)
                        {
                            icon = KillReportEntry.Instance.customIcons[normalizedID].Icon;
                        }
                    }
                }
                WeaponIconImg.gameObject.SetActive(icon != null);
                WeaponIconImg.sprite = icon;
            }
            KillTypeImage.gameObject.SetActive(info.HeadShot);
        }

        void OnMessage(KillReport info)
        {
            DisableAll();
            KillerText.gameObject.SetActive(true);
            KillerText.text = info.Message;
        }

        void OnTeamHighight(KillReport info)
        {
            DisableAll();
            KillerText.gameObject.SetActive(true);
            string hex = ColorUtility.ToHtmlStringRGB(info.KillerTeam.GetTeamColor());
            KillerText.text = string.Format("<color=#{0}>{1}</color> {2}", hex, info.Killer, info.Message);
        }

        void DisableAll()
        {
            SetActiveAll(false);
        }

        void SetActiveAll(bool active)
        {
            KillerText.gameObject.SetActive(active);
            KilledText.gameObject.SetActive(active);
            WeaponText.gameObject.SetActive(active);
            WeaponIconImg.gameObject.SetActive(active);
            KillTypeImage.gameObject.SetActive(active);
        }

        Team GetOppositeTeam(Team team)
        {
            if (isOneTeamMode || team == Team.None) { return team; }

            return (team == Team.Team1) ? Team.Team2 : Team.Team1;
        }

        bool isLocalPlayerName(string playerName) { return playerName == NetworkWrapper.LocalPlayer.NickName; }

        IEnumerator Hide(float time)
        {
            yield return new WaitForSeconds(time);
            while (Alpha.alpha > 0)
            {
                Alpha.alpha -= Time.deltaTime;
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}