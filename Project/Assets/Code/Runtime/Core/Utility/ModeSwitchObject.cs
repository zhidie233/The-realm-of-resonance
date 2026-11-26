using Photon.Pun;
using Resonance.Foundation;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.Multiplayer;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Core
{

    public class ModeSwitchObject : NetworkActorBehaviour
    {
        public GameMode m_GameMode = GameMode.FreeMatch;

        private void Awake()
        {
            if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(GetGameMode == m_GameMode);
        }
    }
}
