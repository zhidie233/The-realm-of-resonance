using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Multiplayer
{

    public class ReadyPlayerUI : MonoBehaviour
    {

        public TextMeshProUGUI NameText;
        public Image LevelImg;
        public Image TeamColorImg;
        public GameObject ReadyUI;
        public GameObject KickButton;
        public GameObject MasterClientUI;
        private Player ThisPlayer;

        public Player GetPlayer()
        {
            return ThisPlayer;
        }

        public void SetInfo(Player player)
        {
            ThisPlayer = player;
            NameText.text = string.Format(player.NickNameAndRole());
            TeamColorImg.color = player.GetPlayerTeam().GetTeamColor();
            MasterClientUI.SetActive(player.IsMasterClient);
            if (KickButton != null) { KickButton.SetActive(false); }
            UpdateState();

            LevelImg.gameObject.SetActive(false);
        }

        public void UpdateState()
        {
            ReadyUI.SetActive(ReadyRoomHandler.Instance.IsPlayerReady(ThisPlayer));
        }

        public void KickThis()
        {
            if (!NetworkWrapper.IsMasterClient) return;

            PhotonNetwork.CloseConnection(ThisPlayer);
        }
    }
}
