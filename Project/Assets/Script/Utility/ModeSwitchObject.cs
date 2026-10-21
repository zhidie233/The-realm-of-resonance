using Photon.Pun;

public class ModeSwitchObject : NetworkHelper
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