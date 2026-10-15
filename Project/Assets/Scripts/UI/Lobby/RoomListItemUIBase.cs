using Photon.Realtime;
using UnityEngine;

namespace Game.Runtime.UI
{
    public abstract class RoomListItemUIBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public abstract void SetInfo(RoomInfo info);
    }
}