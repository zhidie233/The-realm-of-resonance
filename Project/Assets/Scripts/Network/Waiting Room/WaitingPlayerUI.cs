using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class WaitingPlayerUI : WaitingPlayerUIBase
{

    public TextMeshProUGUI NameText;
    public Image LevelImg;
    public Image TeamColorImg;
    public GameObject ReadyUI;
    public GameObject KickButton;
    public GameObject MasterClientUI;
    private Player ThisPlayer;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Player GetPlayer()
    {
        return ThisPlayer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    public override void SetInfo(Player player)
    {
        ThisPlayer = player;
        NameText.text = string.Format(player.NickNameAndRole());
        TeamColorImg.color = player.GetPlayerTeam().GetTeamColor();
        MasterClientUI.SetActive(player.IsMasterClient);
        if(GameConfig.Instance.MasterCanKickPlayers && player.ActorNumber != PhotonNetworkWrapper.LocalPlayer.ActorNumber)
        {
            KickButton.SetActive(PhotonNetworkWrapper.IsMasterClient);
        }
        else { KickButton.SetActive(false); }
        UpdateState();

#if LM
        LevelImg.gameObject.SetActive(true);
        LevelImg.sprite = LevelManager.Instance.GetPlayerLevelInfo(player).Icon;
#else
        LevelImg.gameObject.SetActive(false);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateState()
    {
        ReadyUI.SetActive(WaitingRoomBase.Instance.IsPlayerReady(ThisPlayer));
    }

    /// <summary>
    /// 
    /// </summary>
    public void KickThis()
    {
        if (!PhotonNetworkWrapper.IsMasterClient) return;

        PhotonNetwork.CloseConnection(ThisPlayer);
    }
}