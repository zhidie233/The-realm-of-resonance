using UnityEngine;
using TMPro;

namespace Game.Runtime.UI.Layout
{
    public class RoundFinishScreen : RoundFinishScreenBase
    {
        public GameObject content;
        [SerializeField] private TextMeshProUGUI FinalUIText = null;
        [SerializeField] private TextMeshProUGUI FinalCountText = null;
        [SerializeField] private TextMeshProUGUI FinalWinnerText = null;

        /// <summary>
        /// 显示最后一局 UI
        /// </summary>
        public override void Show(string winner)
        {
            content.SetActive(true);
            FinalUIText.text = (RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht) ? GameTexts.FinalOneMatch.Localized(38) : GameTexts.FinalRounds.Localized(32);
            FinalWinnerText.text = string.Format("{0} {1}", winner, GameTexts.FinalWinner).Localized(33).ToUpper();
        }

        public override void Hide()
        {
            content.SetActive(false);
        }

        public override void SetCountdown(int count)
        {
            count = Mathf.Clamp(count, 0, int.MaxValue);
            FinalCountText.text = count.ToString();
        }
    }
}