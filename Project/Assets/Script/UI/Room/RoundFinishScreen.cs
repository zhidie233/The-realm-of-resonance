using UnityEngine;
using TMPro;

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
           FinalUIText.text = (RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMacht) ? GameTexts.FinalOneMatch : GameTexts.FinalRounds;
           FinalWinnerText.text = string.Format("{0} {1}", winner, GameTexts.FinalWinner).ToUpper();
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