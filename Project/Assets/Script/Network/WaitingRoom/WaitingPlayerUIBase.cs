using Photon.Realtime;
using UnityEngine;

public abstract class WaitingPlayerUIBase : MonoBehaviour
{

    public abstract Player GetPlayer();

    public abstract void SetInfo(Player player);

    public abstract void UpdateState();
}