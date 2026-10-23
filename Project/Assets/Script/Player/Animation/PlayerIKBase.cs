using UnityEngine;

public abstract class PlayerIKBase : MonoBehaviourBase
{
    /// 为 true 时不应使用 IK 控制手臂。
    public bool ControlArmsWithIK
    {
        get;
        set;
    } = true;
    /// 返回玩家 IK 头部注视的 Transform 引用。
    public abstract Transform HeadLookTarget
    { 
        get; 
        set; 
    }
    /// 当 CustomArmsIKHandler != null 时
    /// 继承脚本里应停止用 IK 控制手臂。
    public BodyIKController CustomArmsIKHandler 
    { 
        get; 
        set; 
    } = null;
    /// 初始化 IK 解算器
    public abstract void Init();
}
