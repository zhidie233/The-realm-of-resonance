using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

   public class ContactInfoUI : ContactInfoBase
   {
       public TextMeshProUGUI NameText = null;
       public TextMeshProUGUI StatusText = null;
       public Image StatusImage = null;
       public GameObject JoinButton = null;
       public GameObject expandInfoUI;
       [Space(5)]
       public Color OnlineColor = new Color(0, 0.9f, 0, 0.9f);
       public Color OffLineColor = new Color(0.9f, 0, 0, 0.9f);

       private string roomName = string.Empty;
       private FriendInfo cacheInfo;
       private string OffLineText = "OFFLINE";
       private string OnlineText = "ONLINE";
       private ContactListUIBase UIManager;

       private void OnEnable()
       {
       }


       private void OnDisable()
       {
       }
       
       public override void SetFriendData(FriendInfo info, ContactListUIBase uiManager)
       {
           UIManager = uiManager;
           cacheInfo = info;
           NameText.text = info.UserId;
           UpdateStatusUI(info.IsOnline);
           StatusImage.color = (info.IsOnline) ? OnlineColor : OffLineColor;
           JoinButton.SetActive((info.IsInRoom) ? true : false);
           roomName = info.Room;
           Expand(UIManager.IsOpen());
       }

       public override void UpdateContactInfo(FriendInfo[] infos)
       {
           FriendInfo info = FindMe(infos);
           if (info == null) return;
           NameText.text = info.UserId;
           UpdateStatusUI(info.IsOnline);
           StatusImage.color = (info.IsOnline) ? OnlineColor : OffLineColor;
           JoinButton.SetActive((info.IsInRoom) ? true : false);
           roomName = info.Room;
       }

       public override void Expand(bool isExpanded)
       {
           expandInfoUI.SetActive(isExpanded);
       }

       void UpdateStatusUI(bool online)
       {
           if (StatusText != null) { StatusText.text = string.Format("[{0}]", online ? OnlineText : OffLineText); }
       }

       private FriendInfo FindMe(FriendInfo[] info)
       {
           for (int i = 0; i < info.Length; i++)
           {
               if (info[i].UserId == cacheInfo.UserId)
               {
                   return info[i];
               }
           }

           Destroy(gameObject);
           return null;
       }

       public void JoinRoom()
       {
           if (!string.IsNullOrEmpty(roomName))
           {
               PhotonNetwork.JoinRoom(roomName);
           }
           else
           {
               Debug.Log("This room doesn't exits more");
           }
       }

       public void Remove()
       {
           ContactListBase.Instance.RemoveFriend(NameText.text);
       }

       public override void SetActive(bool active) => gameObject.SetActive(active);
   }