using UnityEngine;
/// 玩家控制器的基类
/// 想用自己的玩家控制器，就让脚本继承该类
/// 并实现需要重写的函数和属性
/// 可参考默认子脚本 FirstPersonController.cs。
public abstract class FirstPersonControllerBase : MonoBehaviourBase
{
    /// 玩家状态必须由子脚本更新
    /// 它始终要表示玩家当前的状态
    public PlayerState State 
    {
        get; 
        set; 
    } = PlayerState.Idle;
    /// 必须由继承类赋值
    /// 返回玩家当前速度
    public abstract Vector3 Velocity
    {
        get;
        set;
    }
    /// 必须由继承类赋值
    /// 返回玩家当前速度大小
    public abstract float VelocityMagnitude
    {
        get; 
        set;
    }
    /// 玩家当前是否由用户输入控制，
    /// 是则返回 true，否则返回 false。
    public abstract bool isControlable
    {
        get;
        set;
    }
    /// 必须由继承类赋值
    /// 玩家是否接触地面，是则返回 true
    /// 通常用 CharacterController.IsGrounded
    public abstract bool isGrounded 
    { 
        get;
    }
    /// 用于访问默认游戏控制器的变量
    /// 如果用自己继承的类，不需要这个。
    private FirstPersonController _gameController = null;
    public FirstPersonController GameController
    {
        get
        {
            if (_gameController == null) TryGetComponent(out _gameController);
            return _gameController;
        }
    }
    /// 让玩家跳跃
    public abstract void DoJump();
    /// 让玩家滑铲
    public abstract void DoSlide();
    /// 开启玩家下坠模式
    public abstract void DoDrop();
    /// 开启玩家滑翔模式
    public abstract void DoGliding();
    /// 强制玩家至少停一帧。
    public abstract void Stop();
    /// 玩家进入平台时处理纵向弹跳
    /// <param name="force">向上力</param>
    public abstract void PlatformJump(float force);
    /// 更新鼠标视角移动
    /// 可手动调用更新。
    public abstract void UpdateMouseLook();
    /// 播放一次脚步声
    /// 播放的音效应与玩家所在的地面材质对应
    /// 即玩家当前踩到的地面
    public abstract void PlayFootStepSound();
    /// 获取 MouseLook 控制器模块
    public abstract MouseLookBase GetMouseLook();
    /// 获取脚步控制器模块
    public abstract Footstep GetFootStep();
    /// 返回玩家在指定状态下的最高速度
    public abstract float GetSpeedOnState(PlayerState playerState, bool includeModifiers = false);
    /// 返回玩家当前速度
    public abstract float GetCurrentSpeed();

    public abstract PlayerRunToAimBehave GetRunToAimBehave();

    public abstract float GetHeadBobMagnitudes(bool verticalMagnitude);

    public abstract float GetSprintFov();
}
