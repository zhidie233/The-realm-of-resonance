using UnityEngine;
using Game.Internal.Interfaces;
using TMPro;
using Game.Runtime.AI;

/// <summary>
/// 默认的游戏赛后结算界面
/// 想用自定义结算界面，只需让脚本实现 IGameResumeScreen 接口
/// 并处理继承来的函数。
/// </summary>
public class GameFinishHandler : PhotonHelper, IGameResumeScreen
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
    [SerializeField] private TextMeshProUGUI CoinsText = null;
    [SerializeField] private GameObject Content = null;

    void OnEnable()
    {
        UIReferences.Instance.ResumeScreen = this;
    }

    public void CollectData()
    {       
        int kills = PhotonNetworkWrapper.LocalPlayer.GetKills();

        // 若机器人击杀不计入玩家数据
        if (GameConfig.Instance.howConsiderBotsEliminations != BotKillConsideration.SameAsRealPlayers)
        {
            if (RoomSettings.TryGetMatchPersistData("bot-kills", out var value))
            {
                int bk = value is int ? (int)value : 0;
                kills = Mathf.Min(0, kills - bk);
            }
        }

        int deaths = PhotonNetworkWrapper.LocalPlayer.GetDeaths();
        int score = PhotonNetworkWrapper.LocalPlayer.GetPlayerScore();
        int kd = kills;
        if (kills <= 0) { kd = -deaths; }
        else if (deaths > 0) { kd = kills / deaths; }
        int timePlayed = Mathf.RoundToInt(GameController.Instance.PlayedTime);
        int scorePerTime = timePlayed * GameConfig.Instance.ScoreReward.ScorePerTimePlayed;
        int hsscore = GameController.Instance.Headshots * GameConfig.Instance.ScoreReward.ScorePerHeadShot;
        bool winner = GameController.Instance.isLocalPlayerWinner();
        int winScore = (winner) ? GameConfig.Instance.ScoreReward.ScoreForWinMatch : 0;
        PlayerNameText.text = PhotonNetworkWrapper.NickName;
        int tscore = score + winScore + scorePerTime;

        int coins = 0;
        if (tscore > 0 && GameConfig.Instance.VirtualCoins.CoinScoreValue > 0 && tscore > GameConfig.Instance.VirtualCoins.CoinScoreValue)
        {
            coins = tscore / GameConfig.Instance.VirtualCoins.CoinScoreValue;
        }
        KillsText.text = string.Format("{0}: <b>{1}</b>", GameTexts.Kills.Localized(126).ToUpper(), kills);
        DeathsText.text = string.Format("{0}: <b>{1}</b>", GameTexts.Deaths.Localized(58, true).ToUpper(), deaths);
        ScoreText.text = string.Format("{0}: <b>{1}</b>", GameTexts.Score.Localized(59).ToUpper(), score - hsscore);
        WinScoreText.text = string.Format(GameTexts.WinMatch.Localized(61), winScore);
        KDRText.text = string.Format("{0}\n<size=10>KDR</size>", kd);
        TimePlayedText.text = string.Format("{0} <b>{1}</b> +{2}", GameTexts.TimePlayed.Localized(60).ToUpper(), StringUtility.GetTimeFormat((float)timePlayed / 60, timePlayed), scorePerTime);
        HeadshotsText.text = string.Format("{0} <b>{1}</b> +{2}", GameTexts.HeadShot.Localized(16, true).ToUpper(), GameController.Instance.Headshots, hsscore);
        TotalScoreText.text = string.Format("{0}\n<size=9>{1}</size>", tscore, GameTexts.TotalScore.Localized(35).ToUpper());
        CoinsText.text = string.Format("+{0}\n<size=9>COINS</size>", coins);

        if (coins > 0)
        {
            GameConfig.Instance.VirtualCoins.AddCoins(coins, PhotonNetworkWrapper.NickName);
        }
        int oldsavedScore = PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKeyForPlayer("score", LocalName), 0);
        oldsavedScore += tscore;
        PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKeyForPlayer("score", LocalName), oldsavedScore);
    }

    public void Show()
    {
        Content.SetActive(true);
        Invoke(nameof(GoToLobby), 60);//maximum time out to leave.
    }

    public void GoToLobby()
    {
        CancelInvoke();
        if (PhotonNetworkWrapper.IsConnected && PhotonNetworkWrapper.InRoom)
        {
            PhotonNetworkWrapper.LeaveRoom();
        }
        else
        {
            UtilityHelper.LoadLevel(GameConfig.Instance.MainMenuScene);
        }
    }
}