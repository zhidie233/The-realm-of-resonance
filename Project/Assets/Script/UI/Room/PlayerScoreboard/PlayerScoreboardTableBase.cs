using Photon.Realtime;
using UnityEngine;

public abstract class PlayerScoreboardTableBase : MonoBehaviour
{

    public abstract PlayerScoreboardUIBase Instance(Player player, GameObject uiPrefab);

    public abstract PlayerScoreboardUIBase InstanceBot(GameBotProperties player, GameObject uiPrefab);

    public abstract void SetActive(bool active);

    public abstract void SetActiveJoinButton(bool active);

    public abstract void ResetJoinButton();
}