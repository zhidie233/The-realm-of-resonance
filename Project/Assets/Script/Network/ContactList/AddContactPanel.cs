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
        ContactsManager.Instance?.AddFriend(nameInput);
        gameObject.SetActive(false);
    }

    private void Add(string friendName)
    {
        logText.text = string.Empty;
        ContactsManager.Instance?.AddFriend(friendName);
        gameObject.SetActive(false);
    }
}