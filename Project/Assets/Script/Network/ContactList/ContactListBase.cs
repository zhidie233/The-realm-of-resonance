using Photon.Realtime;
using TMPro;

   public abstract class ContactListBase : NetworkHelper
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

       private static ContactListBase _instance;
       public static ContactListBase Instance
       {
           get
           {
               if (_instance == null)
               {
                   _instance = FindObjectOfType<ContactListBase>();
               }
               return _instance;
           }
       }
   }