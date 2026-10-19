using Photon.Realtime;
using TMPro;

namespace Game.Runtime.FriendList
{
    public abstract class FriendListBase : NetworkHelper
    {
        public enum Status
        {
            Idle,
            Fetching
        }

        public abstract Status GetStatus();

        public abstract void AddFriend(string friend);

        public abstract void AddFriend(TMP_InputField field);

        public abstract void RemoveFriend(string friend);

        public abstract bool IsPlayerFriend(string playerNick);

        public abstract bool CanAddMoreFriends();

        public abstract FriendInfo[] GetFriends();

        public abstract int FriendsCount { get; }

        private static FriendListBase _instance;
        public static FriendListBase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FriendListBase>();
                }
                return _instance;
            }
        }
    }
}