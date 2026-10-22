using Photon.Realtime;
using UnityEngine;

   public abstract class ContactInfoBase : MonoBehaviour
   {

       public abstract void SetFriendData(FriendInfo info, ContactListUIBase uiManager);

       public abstract void UpdateContactInfo(FriendInfo[] infos);

       public abstract void SetActive(bool active);

       public abstract void Expand(bool expand);
   }