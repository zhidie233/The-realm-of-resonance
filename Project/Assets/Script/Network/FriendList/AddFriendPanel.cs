﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Runtime.FriendList
{
    public class AddFriendPanel : MonoBehaviour
    {
        public TMP_InputField nameInput;
        public TextMeshProUGUI logText;
        public Button addButton;

        /// <summary>
        /// 
        /// </summary>
        public void AddFriend()
        {
            FriendListBase.Instance?.AddFriend(nameInput);
            gameObject.SetActive(false);
        }

        private void Add(string friendName)
        {
            logText.text = string.Empty;
            FriendListBase.Instance?.AddFriend(friendName);
            gameObject.SetActive(false);
        }
    }
}