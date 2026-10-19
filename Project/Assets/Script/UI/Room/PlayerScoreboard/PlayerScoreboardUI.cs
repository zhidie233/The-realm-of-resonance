using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using Game.Runtime.AI;

public class PlayerScoreboardUI : PlayerScoreboardUIBase
{
    #region Public members
    [SerializeField] private TextMeshProUGUI NameText = null;
    [SerializeField] private TextMeshProUGUI KillsText = null;
    [SerializeField] private TextMeshProUGUI DeathsText = null;
    [SerializeField] private TextMeshProUGUI ScoreText = null;
    [SerializeField] private GameObject KickButton = null;
    [SerializeField] private Image LevelIcon = null;
    public TextMeshProUGUI levelNumberText;
    public GameObject localHighlight;
    #endregion

    #region Private members
    private bool isInitializated = false;
    private Team InitTeam = Team.None;
    #endregion

    /// <summary>
    /// 该玩家首次出现在记分板时调用
    /// </summary>
    public override void Init(Player player, GameBotProperties bot = null)
    {
        Bot = bot;
        isBotBinding = bot != null;

        if (Bot != null)
        {
            UpdateBot();
            return;
        }

        RealPlayer = player;
        gameObject.name = player.NickName + player.ActorNumber;
        InitTeam = player.GetPlayerTeam();
        UpdatePlayerUI(RealPlayer);

        LevelIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// 记分板每次刷新时调用（记分板打开期间）
    /// </summary>
    public override bool Refresh()
    {
        if (Bot != null || isBotBinding) {  return UpdateBot(); }

        if (RealPlayer == null || RealPlayer.GetPlayerTeam() != InitTeam)
        {
            if (!PlayerScoreboardBase.Instance.RemoveUIBinding(this))
            {
                Destroy();
            }
            return false;
        }

        UpdatePlayerUI(RealPlayer);
        return true;
    }

    private void UpdatePlayerUI(Player player)
    {
        NameText.text = player.NickNameAndRole();
        if (!player.CustomProperties.ContainsKey(NetworkKeys.KillsKey)) return;

        if (localHighlight != null) localHighlight.SetActive(player.IsLocal);
        KillsText.text = player.GetKills().ToString();
        DeathsText.text = player.GetDeaths().ToString();
        ScoreText.text = player.GetPlayerScore().ToString();

        if (GameConfig.Instance.MasterCanKickPlayers && player.ActorNumber != NetworkWrapper.LocalPlayer.ActorNumber)
        {
            if (KickButton != null) KickButton.SetActive(NetworkWrapper.IsMasterClient);
        }
        else { if (KickButton != null) KickButton.SetActive(false); }

    }

    public override bool UpdateBot()
    {
        if (Bot == null || string.IsNullOrEmpty(Bot.Name) || !AIManager.Instance.BotsStatistics.Exists(x => x.Name == Bot.Name))
        {
            if (!PlayerScoreboardBase.Instance.RemoveUIBinding(this))
            {
                Destroy();
                return false;
            }
        }

        gameObject.name = Bot.Name;
        if (localHighlight != null) localHighlight.SetActive(false);
        NameText.text = Bot.Name;
        KillsText.text = Bot.Kills.ToString();
        DeathsText.text = Bot.Deaths.ToString();
        ScoreText.text = Bot.Score.ToString();
        InitTeam = Bot.Team;
        if (KickButton != null) KickButton.SetActive(false);


        return true;
    }

    public void Kick()
    {
        if (NetworkWrapper.IsMasterClient)
        {
            NetworkWrapper.Instance.KickPlayer(RealPlayer);
        }
    }

    public void OnClick()
    {
        var player = Bot == null ? RealPlayer : null;
        PlayerScoreboardBase.Instance.OnPlayerClicked(player);
    }

    void OnEnable()
    {
        if (RealPlayer == null && !isBotBinding && isInitializated)
        {
            Destroy(gameObject);
            isInitializated = true;
        }
        else if (isBotBinding && Bot == null && isInitializated)
        {
            Destroy(gameObject);
            isInitializated = true;
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public override int GetScore()
    {
        if (Bot == null) { return RealPlayer.GetPlayerScore(); }
        else { return Bot.Score; }
    }

    public override Team GetTeam()
    {
        return InitTeam;
    }
}