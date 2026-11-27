using Photon.Pun;
using Resonance.Foundation;
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
