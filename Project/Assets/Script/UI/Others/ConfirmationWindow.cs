using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

   public class ConfirmationWindow : MonoBehaviour
   {
       public Text descriptionText;
       public TextMeshProUGUI descriptionTextTMP;
       public TextMeshProUGUI confirmationTextTMP;
       public GameObject content;

       [Header("Events")]
       public GameEvents.UEvent onConfirm;
       public GameEvents.UEvent onCancel;

       private Action callback;
       private Action cancelCallback;

       public void AskConfirmation(string description, Action onAccept, Action onCancel = null)
       {
           callback = onAccept;
           cancelCallback = onCancel;
           if (!string.IsNullOrEmpty(description))
           {
               if (descriptionText != null) descriptionText.text = description;
               if (descriptionTextTMP != null) descriptionTextTMP.text = description;
           }

           content.SetActive(true);
       }

       public void ShowMessage(string description)
       {
           if (!string.IsNullOrEmpty(description))
           {
               if (descriptionText != null) descriptionText.text = description;
               if (descriptionTextTMP != null) descriptionTextTMP.text = description;
           }

           content.SetActive(true);
       }

       public void SetConfirmationText(string text)
       {
           if (confirmationTextTMP != null) confirmationTextTMP.text = text;
       }

       public void Confirm()
       {
           callback?.Invoke();
           onConfirm?.Invoke();
           content.SetActive(false);
       }

       public void Cancel()
       {
           callback = null;
           cancelCallback?.Invoke();
           onCancel?.Invoke();
           cancelCallback = null;
           content.SetActive(false);
       }
   }