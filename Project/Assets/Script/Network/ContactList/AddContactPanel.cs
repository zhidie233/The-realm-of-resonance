using UnityEngine;
using UnityEngine.UI;
using TMPro;

   public class AddContactPanel : MonoBehaviour
   {
       public TMP_InputField nameInput;
       public TextMeshProUGUI logText;
       public Button addButton;

       public void AddFriend()
       {
           ContactListBase.Instance?.AddFriend(nameInput);
           gameObject.SetActive(false);
       }

       private void Add(string friendName)
       {
           logText.text = string.Empty;
           ContactListBase.Instance?.AddFriend(friendName);
           gameObject.SetActive(false);
       }
   }