using Photon.Realtime;
using UnityEngine;

namespace Game.Runtime.UI
{
    public abstract class RoomListItemUIBase : MonoBehaviour
    {
        public abstract void SetInfo(RoomInfo info);
    }
}