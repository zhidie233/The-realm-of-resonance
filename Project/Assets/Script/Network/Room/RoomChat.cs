using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;

public class RoomChat : RoomChatBase
{
    public int messageBufferLenght = 7;
    [Header("References")]
    public GameObject chatUIRoot;
    public TMP_InputField chatInputField;
    public TextMeshProUGUI chatText;

    public bool CanUseTheChat { get; set; } = true;
    private List<string> messages = new List<string>();
    private MessageTarget messageTarget = MessageTarget.All;

    protected override void Awake()
    {
        base.Awake();
        GameConfig.Instance.isChating = false;
        this.InvokeAfter(5, HideChat);
        if (chatInputField != null) chatInputField.gameObject.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        PhotonNetworkWrapper.Instance.AddCallback(PropertiesKeys.ChatEvent, OnNetworkMessageReceived);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (PhotonNetworkWrapper.Instance != null)
            PhotonNetworkWrapper.Instance.RemoveCallback(OnNetworkMessageReceived);
    }

    private void Start()
    {
        SetChatLocally(GameTexts.OpenChatStart);
    }

    public override void OnUpdate()
    {
        if (!CanUseTheChat) return;

        if (!GameConfig.Instance.isChating)
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

    /// <summary>
    /// 接收服务器聊天事件发来的网络消息
    /// </summary>
    /// <param name="data"></param>
    void OnNetworkMessageReceived(ExitGames.Client.Photon.Hashtable data)
    {
        var messageTarget = (MessageTarget)data["target"];
        var sender = (Player)data["sender"];
        var message = (string)data["chat"];

        if (messageTarget == MessageTarget.Team && sender.GetPlayerTeam() != PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam())//check if team message and only team receive it
            return;

        string senderName = "anonymous";

        if (sender != null)
        {
            senderName = string.IsNullOrEmpty(sender.NickName) ? $"Player {sender.ActorNumber}" : sender.NickName;
        }

        string txt = string.Format("[{0}] [{1}]:{2}", messageTarget.ToString().ToUpper(), senderName, message);
        this.messages.Add(txt);
        if (messages.Count > messageBufferLenght)
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

    /// <summary>
    /// 通过网络向其他所有客户端发送文本消息
    /// </summary>
    /// <param name="txt"></param>
    public override void SetChat(string txt)
    {
        if (!PhotonNetworkWrapper.IsConnected || !CanUseTheChat)
            return;
        if (string.IsNullOrEmpty(txt)) return;

        var data = UtilityHelper.CreatePhotonHashTable();
        data.Add("sender", PhotonNetworkWrapper.LocalPlayer);
        data.Add("target", messageTarget);
        data.Add("chat", txt);

        PhotonNetworkWrapper.Instance.SendDataOverNetwork(PropertiesKeys.ChatEvent, data);
    }

    /// <summary>
    /// 仅在本地聊天窗口发布消息。
    /// </summary>
    /// <param name="newLine"></param>
    public override void SetChatLocally(string newLine)
    {
        this.messages.Add(newLine);
        if (messages.Count > messageBufferLenght)
        {
            messages.RemoveAt(0);
        }
    }

    public override void SetActiveUI(bool active)
    {
        chatInputField.gameObject.SetActive(active);
    }

    public override void OnSubmit()
    {
        GameConfig.Instance.isChating = !GameConfig.Instance.isChating;
        chatInputField.gameObject.SetActive(GameConfig.Instance.isChating);

        if (GameConfig.Instance.isChating)
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