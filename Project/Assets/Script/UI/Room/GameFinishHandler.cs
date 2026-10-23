using UnityEngine;
using TMPro;
/// 默认的游戏赛后结算界面
/// 想用自定义结算界面，只需让脚本实现 IGameResumeScreen 接口
/// 并处理继承来的函数。
public class GameFinishHandler : NetworkHelper, IGameResumeScreen
{

    [SerializeField] private TextMeshProUGUI PlayerNameText = null;
    [SerializeField] private TextMeshProUGUI KillsText = null;
    [SerializeField] private TextMeshProUGUI DeathsText = null;
    [SerializeField] private TextMeshProUGUI ScoreText = null;
    [SerializeField] private TextMeshProUGUI KDRText = null;
    [SerializeField] private TextMeshProUGUI TimePlayedText = null;
    [SerializeField] private TextMeshProUGUI WinScoreText = null;
    [SerializeField] private TextMeshProUGUI HeadshotsText = null;
    [SerializeField] private TextMeshProUGUI TotalScoreText = null;
    [SerializeField] private GameObject Content = null;

    void OnEnable()
    {
        UIReferences.Instance.ResumeScreen = this;
    }

    public void CollectData()
    {       
        int kills = NetworkWrapper.LocalPlayer.GetKills();
        // 若机器人击杀不计入玩家数据
        if (GameConfig.Instance.botKillCredit != BotKillPolicy.SameAsRealPlayers)
        {
            if (RoomSettings.TryGetMatchPersistData("bot-kills", out var value))
            {
                int bk = value is int ? (int)value : 0;
                kills = Mathf.Min(0, kills - bk);
            }
        }

        int deaths = NetworkWrapper.LocalPlayer.GetDeaths();
        int score = NetworkWrapper.LocalPlayer.GetPlayerScore();
        int kd = kills;
        if (kills <= 0) { kd = -deaths; }
        else if (deaths > 0) { kd = kills / deaths; }
        int timePlayed = Mathf.RoundToInt(GameController.Instance.PlayedTime);
        int scorePerTime = timePlayed * GameConfig.Instance.matchScore.PerMinuteScore;
        int hsscore = GameController.Instance.Headshots * GameConfig.Instance.matchScore.HeadshotExtraReward;
        bool winner = GameController.Instance.isLocalPlayerWinner();
        int winScore = (winner) ? GameConfig.Instance.matchScore.WinMatchReward : 0;
        PlayerNameText.text = NetworkWrapper.NickName;
        int tscore = score + winScore + scorePerTime;

        KillsText.text = string.Format("{0}: <b>{1}</b>", GameTexts.Kills.ToUpper(), kills);
        DeathsText.text = string.Format("{0}: <b>{1}</b>", GameTexts.Deaths.ToUpper(), deaths);
        ScoreText.text = string.Format("{0}: <b>{1}</b>", GameTexts.Score.ToUpper(), score - hsscore);
        WinScoreText.text = string.Format(GameTexts.WinMatch, winScore);
        KDRText.text = string.Format("{0}\n<size=10>KDR</size>", kd);
        TimePlayedText.text = string.Format("{0} <b>{1}</b> +{2}", GameTexts.TimePlayed.ToUpper(), StringHelpers.GetTimeFormat((float)timePlayed / 60, timePlayed), scorePerTime);
        HeadshotsText.text = string.Format("{0} <b>{1}</b> +{2}", GameTexts.HeadShot.ToUpper(), GameController.Instance.Headshots, hsscore);
        TotalScoreText.text = string.Format("{0}\n<size=9>{1}</size>", tscore, GameTexts.TotalScore.ToUpper());

        int oldsavedScore = PlayerPrefs.GetInt(NetworkKeys.GetUniqueKeyForPlayer("score", LocalName), 0);
        oldsavedScore += tscore;
        PlayerPrefs.SetInt(NetworkKeys.GetUniqueKeyForPlayer("score", LocalName), oldsavedScore);
    }

    public void Show()
    {
        Content.SetActive(true);
        Invoke(nameof(GoToLobby), 60);
    }

    public void GoToLobby()
    {
        CancelInvoke();
        if (NetworkWrapper.IsConnected && NetworkWrapper.InRoom)
        {
            NetworkWrapper.LeaveRoom();
        }
        else
        {
            CommonUtil.LoadLevel(GameConfig.Instance.MainMenuScene);
        }
    }
}
