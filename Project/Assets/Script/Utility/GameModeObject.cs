using Photon.Pun;

public class GameModeObject : NetworkHelper
{
    public GameMode m_GameMode = GameMode.FFA;

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