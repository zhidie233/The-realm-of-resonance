using UnityEngine;

   public abstract class ContactListUIBase : MonoBehaviour
   {

       public abstract void UpdateContactList(bool rebuild = false);

       public abstract void ShowMessage(string message);

       public abstract void SetActiveList(bool active);

       public abstract bool IsOpen();

       public abstract bool IsPlayerListed(string playerNick);
   }