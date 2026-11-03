using UnityEngine;
using TMPro;

public class RoundFinishScreen : MonoBehaviour
{
    private static RoundFinishScreen _instance;
    public static RoundFinishScreen Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<RoundFinishScreen>();
            return _instance;
        }
    }

    public GameObject content;
    [SerializeField] private TextMeshProUGUI FinalUIText = null;
    [SerializeField] private TextMeshProUGUI FinalCountText = null;
    [SerializeField] private TextMeshProUGUI FinalWinnerText = null;
    /// 显示最后一局 UI
    public void Show(string winner)
    {
        content.SetActive(true);
        FinalUIText.text = (RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMatch) ? GameTexts.FinalOneMatch : GameTexts.FinalRounds;
        FinalWinnerText.text = string.Format("{0} {1}", winner, GameTexts.FinalWinner).ToUpper();
    }

    public void Hide()
    {
        content.SetActive(false);
    }

    public void SetCountdown(int count)
    {
        count = Mathf.Clamp(count, 0, int.MaxValue);
        FinalCountText.text = count.ToString();
    }
}
