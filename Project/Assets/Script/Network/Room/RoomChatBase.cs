public abstract class RoomChatBase : MonoBehaviourBase
{
    private bool _isChatAvailable = true;
    public bool IsChatAvailable
    {
        get => _isChatAvailable;
        set => _isChatAvailable = value;
    }

    /// <summary>
    /// 本机玩家向所有人发送文本
    /// </summary>
    /// <param name="text"></param>
    public abstract void SetChat(string text);

    /// <summary>
    /// 仅本地客户端
    /// </summary>
    /// <param name="text"></param>
    public abstract void SetChatLocally(string text);

    /// <summary>
    /// 应从输入框取文本
    /// 然后做与 SetChat(...) 相同的事
    /// </summary>
    public abstract void OnSubmit();

    /// <summary>
    /// 启用/关闭聊天 UI。
    /// </summary>
    public abstract void SetActiveUI(bool active);

    private static RoomChatBase _instance;
    public static RoomChatBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<RoomChatBase>(); }
            return _instance;
        }
    }
}