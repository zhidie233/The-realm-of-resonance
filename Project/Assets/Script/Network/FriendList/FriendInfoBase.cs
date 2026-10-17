using Photon.Realtime;
using UnityEngine;

namespace Game.Runtime.FriendList
{
    public abstract class FriendInfoBase : MonoBehaviour
    {

        public abstract void SetFriendData(FriendInfo info, FriendListUIBase uiManager);

        public abstract void UpdateFriendInfo(FriendInfo[] infos);

        public abstract void SetActive(bool active);

        public abstract void Expand(bool expand);
    }
}