using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Combat;
using Resonance.Actors;
using Resonance.Bots;
using Resonance.Match;
using Resonance.World;
using Resonance.Anomaly;
using Resonance.UI;
namespace Resonance.Multiplayer
{

    public class RoomChat : MonoBehaviour
    {
        private bool _isChatAvailable = true;
        public bool IsChatAvailable
        {
            get => _isChatAvailable;
            set => _isChatAvailable = value;
        }

        private static RoomChat _instance;
        public static RoomChat Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<RoomChat>(); }
                return _instance;
            }
        }

        public int messageBufferLength = 7;
        [Header("References")]
        public GameObject chatUIRoot;
        public TMP_InputField chatInputField;
        public TextMeshProUGUI chatText;

        public bool CanUseTheChat { get; set; } = true;
        private List<string> messages = new List<string>();
        private MessageTarget messageTarget = MessageTarget.All;

        protected virtual void Awake()
        {
            ProjectConfigRoot.Instance.isChatOpen = false;
            this.InvokeAfter(5, HideChat);
            if (chatInputField != null) chatInputField.gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            NetworkWrapper.Instance.AddCallback(NetProtocol.ChatEvent, OnNetworkMessageReceived);
        }

        protected virtual void OnDisable()
        {
            if (NetworkWrapper.Instance != null)
                NetworkWrapper.Instance.RemoveCallback(OnNetworkMessageReceived);
        }

        private void Start()
        {
            SetChatLocally(GameTexts.OpenChatStart);
        }

        protected virtual void Update()
        {
            if (!CanUseTheChat) return;

            if (!ProjectConfigRoot.Instance.isChatOpen)
            {
                if (GameInput.GeneralChat())
                {
                    messageTarget = MessageTarget.All;
                    OnSubmit();
                }
                if (GameInput.TeamChat())
                {
                    messageTarget = MessageTarget.Team;
                    OnSubmit();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    OnSubmit();
                }
            }
        }
        /// 接收服务器聊天事件发来的网络消息
        void OnNetworkMessageReceived(ExitGames.Client.Photon.Hashtable data)
        {
            var messageTarget = (MessageTarget)data["target"];
            var sender = (Player)data["sender"];
            var message = (string)data["chat"];
            if (messageTarget == MessageTarget.Team && sender.GetPlayerTeam() != NetworkWrapper.LocalPlayer.GetPlayerTeam())
                return;

            string senderName = "anonymous";

            if (sender != null)
            {
                senderName = string.IsNullOrEmpty(sender.NickName) ? $"Player {sender.ActorNumber}" : sender.NickName;
            }

            string txt = string.Format("[{0}] [{1}]:{2}", messageTarget.ToString().ToUpper(), senderName, message);
            this.messages.Add(txt);
            if (messages.Count > messageBufferLength)
            {
                messages.RemoveAt(0);
            }

            chatText.text = string.Empty;
            foreach (string m in messages)
            {
                chatText.text += m + "\n";
            }
            CancelInvoke(nameof(HideChat));
            chatText.CrossFadeAlpha(1, 0.3f, true);
            Invoke(nameof(HideChat), 5);
        }
        /// 通过网络向其他所有客户端发送文本消息
        public void SetChat(string txt)
        {
            if (!NetworkWrapper.IsConnected || !CanUseTheChat)
                return;
            if (string.IsNullOrEmpty(txt)) return;

            var data = NetPropertyStore.CreateBag();
            data.Add("sender", NetworkWrapper.LocalPlayer);
            data.Add("target", messageTarget);
            data.Add("chat", txt);

            NetworkWrapper.Instance.SendDataOverNetwork(NetProtocol.ChatEvent, data);
        }
        /// 仅在本地聊天窗口发布消息。
        public void SetChatLocally(string newLine)
        {
            this.messages.Add(newLine);
            if (messages.Count > messageBufferLength)
            {
                messages.RemoveAt(0);
            }
        }

        /// 启用/关闭聊天 UI。
        public void SetActiveUI(bool active)
        {
            chatInputField.gameObject.SetActive(active);
        }

        /// 应从输入框取文本
        /// 然后做与 SetChat(...) 相同的事
        public void OnSubmit()
        {
            ProjectConfigRoot.Instance.isChatOpen = !ProjectConfigRoot.Instance.isChatOpen;
            chatInputField.gameObject.SetActive(ProjectConfigRoot.Instance.isChatOpen);

            if (ProjectConfigRoot.Instance.isChatOpen)
            {
                chatInputField.text = string.Empty;
                chatInputField.ActivateInputField();
            }
            else
            {
                SetChat(chatInputField.text);
                chatInputField.DeactivateInputField();
            }
        }

        public void HideChat()
        {
            chatText.CrossFadeAlpha(0, 2, true);
        }

        public void Refresh()
        {
            chatText.text = string.Empty;
            foreach (string m in messages)
                chatText.text += m + "\n";
        }

        public void SubmitInputField()
        {
            OnSubmit();
        }

        public enum MessageTarget
        {
            All,
            Team,
        }
    }
}
