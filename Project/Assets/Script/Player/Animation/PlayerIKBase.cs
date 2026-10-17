using UnityEngine;

public abstract class PlayerIKBase : MonoBehaviourBase
{
    /// <summary>
    /// 为 true 时不应使用 IK 控制手臂。
    /// </summary>
    public bool ControlArmsWithIK
    {
        get;
        set;
    } = true;

    /// <summary>
    /// 返回玩家 IK 头部注视的 Transform 引用。
    /// </summary>
    /// <returns></returns>
    public abstract Transform HeadLookTarget
    { 
        get; 
        set; 
    }

    /// <summary>
    /// 当 CustomArmsIKHandler != null 时
    /// 继承脚本里应停止用 IK 控制手臂。
    /// </summary>
    public BodyIKHandler CustomArmsIKHandler 
    { 
        get; 
        set; 
    } = null;

    /// <summary>
    /// 初始化 IK 解算器
    /// </summary>
    public abstract void Init();
}