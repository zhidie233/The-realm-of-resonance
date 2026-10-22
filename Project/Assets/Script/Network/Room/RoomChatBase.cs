public abstract class RoomChatBase : MonoBehaviourBase
{
    private bool _isChatAvailable = true;
    public bool IsChatAvailable
    {
        get => _isChatAvailable;
        set => _isChatAvailable = value;
    }
    /// 本机玩家向所有人发送文本
    public abstract void SetChat(string text);
    /// 仅本地客户端
    public abstract void SetChatLocally(string text);
    /// 应从输入框取文本
    /// 然后做与 SetChat(...) 相同的事
    public abstract void OnSubmit();
    /// 启用/关闭聊天 UI。
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
