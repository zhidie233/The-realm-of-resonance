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

    public override Player GetPlayer()
    {
        return ThisPlayer;
    }

    public override void SetInfo(Player player)
    {
        ThisPlayer = player;
        NameText.text = string.Format(player.NickNameAndRole());
        TeamColorImg.color = player.GetPlayerTeam().GetTeamColor();
        MasterClientUI.SetActive(player.IsMasterClient);
        if(GameConfig.Instance.MasterCanKickPlayers && player.ActorNumber != NetworkWrapper.LocalPlayer.ActorNumber)
        {
            KickButton.SetActive(NetworkWrapper.IsMasterClient);
        }
        else { KickButton.SetActive(false); }
        UpdateState();

        LevelImg.gameObject.SetActive(false);
    }

    public override void UpdateState()
    {
        ReadyUI.SetActive(WaitingRoomBase.Instance.IsPlayerReady(ThisPlayer));
    }

    public void KickThis()
    {
        if (!NetworkWrapper.IsMasterClient) return;

        PhotonNetwork.CloseConnection(ThisPlayer);
    }
}