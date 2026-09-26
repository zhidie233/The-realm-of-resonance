using Photon.Realtime;
using UnityEngine;

namespace GFWK.Runtime.UI
{
    public abstract class bl_RoomListItemUIBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public abstract void SetInfo(RoomInfo info);
    }
}