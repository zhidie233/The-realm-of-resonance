using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

   public class ContactsManager : ContactListBase, IMatchmakingCallbacks, IConnectionCallbacks, ILobbyCallbacks
   {
       private List<string> friendsNames = new List<string>();
       private ContactListUIBase FriendUI;
       private char splitChar = '/';
       private bool firstBuild = false;
       private List<FriendInfo> contactList = new List<FriendInfo>();
       private Status m_status = Status.Idle;

       public override int FriendsCount => contactList.Count;

       void Awake()
       {
           FriendUI = LobbyUI.Instance.FriendUI;
           PhotonNetwork.AddCallbackTarget(this);
           if (NetworkWrapper.IsConnected && !string.IsNullOrEmpty(NetworkWrapper.NickName))
           {
               GetFriendsStore();
               InvokeRepeating(nameof(UpdateList), 1, 1);
           }
       }

       void OnDisable()
       {
           PhotonNetwork.RemoveCallbackTarget(this);
       }

       public override Status GetStatus()
       {
           return m_status;
       }

       void GetFriendsStore()
       {
           if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby)
               return;

           friendsNames.Clear();
           //获取已保存的所有好友
           string cacheFriend = PlayerPrefs.GetString(SaveKey, "Null");
           if (!string.IsNullOrEmpty(cacheFriend))
           {
               string[] splitFriends = cacheFriend.Split(splitChar);
               friendsNames.AddRange(splitFriends);
           }
           //在 Photon 列表中查找所有好友名字
           if (friendsNames.Count > 0)
           {
               PhotonNetwork.FindFriends(friendsNames.ToArray());
               //更新列表 UI
               FriendUI.UpdateContactList(true);
           }
           else
           {
               return;
           }
       }
       /// 调用以更新好友列表
       void UpdateList()
       {
           if (!NetworkWrapper.IsConnected || NetworkWrapper.InRoom)
               return;

           if (friendsNames.Count > 0)
           {
               if (friendsNames.Count > 1 && friendsNames.Contains("Null"))
               {
                   friendsNames.Remove("Null");
                   friendsNames.Remove("Null");
                   SaveFriends();
               }
               if (PhotonNetwork.IsConnectedAndReady && friendsNames.Count > 0)
               {
                   PhotonNetwork.FindFriends(friendsNames.ToArray());
               }
           }
       }

       public void SaveFriends()
       {
           string allfriends = string.Join(splitChar.ToString(), friendsNames.ToArray());
           PlayerPrefs.SetString(SaveKey, allfriends);
       }

       public override void AddFriend(TMP_InputField field)
       {
           AddFriend(field.text);
       }

       public override void AddFriend(string friend)
       {

           if (friendsNames.Contains(friend)) return;
           if (friendsNames.Count > GameConfig.Instance.contactLimit)
           {
               FriendUI.ShowMessage("Max friends reached!");
               return;
           }
           string t = friend;
           if (string.IsNullOrEmpty(t))
               return;

           if (FriendUI != null && FriendUI.IsPlayerListed(t))
           {
               FriendUI.ShowMessage("Already has added this friend.");
               return;
           }
           if (t == NetworkWrapper.NickName)
           {
               FriendUI.ShowMessage("You can't add yourself.");
               return;
           }

           friendsNames.Add(friend);
           PhotonNetwork.FindFriends(friendsNames.ToArray());
           FriendUI.UpdateContactList(true);
           SaveFriends();
           m_status = Status.Fetching;
       }

       public override void RemoveFriend(string friend)
       {
           if (friendsNames.Contains(friend))
           {
               friendsNames.Remove(friend);
               SaveFriends();
               if (friendsNames.Count > 0)
               {
                   if (friendsNames.Count > 1 && friendsNames.Contains("Null"))
                   {
                       friendsNames.Remove("Null");
                       friendsNames.Remove("Null");
                       SaveFriends();
                   }
                   if (friendsNames.Count > 0)
                       PhotonNetwork.FindFriends(friendsNames.ToArray());
               }
               else
               {
                   AddFriend("Null");
                   if (friendsNames.Count > 0)
                       PhotonNetwork.FindFriends(friendsNames.ToArray());
               }

               FriendUI.UpdateContactList(true);
               m_status = Status.Fetching;
           }
           else {  }
       }

       public override FriendInfo[] GetFriends()
       {
           return contactList.ToArray();
       }

       public override bool IsPlayerFriend(string playerNick)
       {
           return friendsNames.Contains(playerNick);
       }

       public override bool CanAddMoreFriends()
       {
           return friendsNames.Count < GameConfig.Instance.contactLimit;
       }

       public void OnFriendListUpdate(List<FriendInfo> contactList)
       {
           if (string.IsNullOrEmpty(NetworkWrapper.NickName)) return;

           // 修复第一个添加的好友现在才出现在列表里的问题
           foreach (var item in contactList)
           {
               if (item.UserId == NetworkWrapper.NickName) return;
           }

           bool build = (contactList.Count != this.contactList.Count);
           if (contactList.Count == 1)
           {
               if (contactList[0].UserId != "Null")
               {
                   if (!firstBuild) { build = true; firstBuild = true; }
               }
               else firstBuild = false;

           }
           else firstBuild = false;

           this.contactList = contactList;
           FriendUI.UpdateContactList(build);
       }

       public void StartFriendsUpdate()
       {
           GetFriendsStore();
           CancelInvoke();
           InvokeRepeating(nameof(UpdateList), 1, 1);
           FriendUI.SetActiveList(true);
       }
       /// 每个玩家的自定义 key，可在同一设备上保存多份好友列表
       private string SaveKey
       {
           get
           {
               return NetworkKeys.GetUniqueKeyForPlayer("resonancerealm.friends", NetworkWrapper.NickName);
           }
       }

#region Photon Callbacks
       public void OnCreatedRoom()
       {

       }

       public void OnCreateRoomFailed(short returnCode, string message)
       {

       }

       public void OnJoinRoomFailed(short returnCode, string message)
       {

       }

       public void OnJoinRandomFailed(short returnCode, string message)
       {

       }

       public void OnLeftRoom()
       {

       }

       public void OnJoinedRoom()
       {
           CancelInvoke(nameof(UpdateList));
       }

       public void OnConnected()
       {

       }

       public void OnConnectedToMaster()
       {

       }

       public void OnJoinedLobby()
       {
           if (!string.IsNullOrEmpty(NetworkWrapper.NickName))
           {
               StartFriendsUpdate();
           }
       }

       public void OnDisconnected(DisconnectCause cause)
       {
           CancelInvoke(nameof(UpdateList));
       }

       public void OnRegionListReceived(RegionHandler regionHandler)
       {

       }

       public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
       {

       }

       public void OnCustomAuthenticationFailed(string debugMessage)
       {

       }

       public void OnLeftLobby()
       {
           CancelInvoke(nameof(UpdateList));
       }

       public void OnRoomListUpdate(List<RoomInfo> roomList)
       {

       }

       public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
       {

       }
#endregion
   }
