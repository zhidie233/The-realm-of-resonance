using Photon.Realtime;
using UnityEngine;

   public abstract class ContactInfoBase : MonoBehaviour
   {

       public abstract void SetFriendData(ContactInfo info, ContactListUIBase uiManager);

       public abstract void UpdateContactInfo(ContactInfo[] infos);

       public abstract void SetActive(bool active);

       public abstract void Expand(bool expand);
   }