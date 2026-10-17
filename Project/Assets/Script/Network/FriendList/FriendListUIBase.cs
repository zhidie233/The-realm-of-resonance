using UnityEngine;

namespace Game.Runtime.FriendList
{
    public abstract class FriendListUIBase : MonoBehaviour
    {

        public abstract void UpdateFriendList(bool rebuild = false);

        public abstract void ShowMessage(string message);

        public abstract void SetActiveList(bool active);

        public abstract bool IsOpen();

        public abstract bool IsPlayerListed(string playerNick);
    }
}