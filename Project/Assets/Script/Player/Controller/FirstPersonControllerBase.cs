using UnityEngine;

/// <summary>
/// 玩家控制器的基类
/// 想用自己的玩家控制器，就让脚本继承该类
/// 并实现需要重写的函数和属性
/// 可参考默认子脚本 FirstPersonController.cs。
/// </summary>
public abstract class FirstPersonControllerBase : MonoBehaviourBase
{
    /// <summary>
    /// 玩家状态必须由子脚本更新
    /// 它始终要表示玩家当前的状态
    /// </summary>
    public PlayerState State 
    {
        get; 
        set; 
    } = PlayerState.Idle;

    /// <summary>
    /// 必须由继承类赋值
    /// 返回玩家当前速度
    /// </summary>
    public abstract Vector3 Velocity
    {
        get;
        set;
    }

    /// <summary>
    /// 必须由继承类赋值
    /// 返回玩家当前速度大小
    /// </summary>
    public abstract float VelocityMagnitude
    {
        get; 
        set;
    }

    /// <summary>
    /// 玩家当前是否由用户输入控制，
    /// 是则返回 true，否则返回 false。
    /// </summary>
    public abstract bool isControlable
    {
        get;
        set;
    }

    /// <summary>
    /// 必须由继承类赋值
    /// 玩家是否接触地面，是则返回 true
    /// 通常用 CharacterController.IsGrounded
    /// </summary>
    public abstract bool isGrounded 
    { 
        get;
    }

    /// <summary>
    /// 用于访问默认游戏控制器的变量
    /// 如果用自己继承的类，不需要这个。
    /// </summary>
    private FirstPersonController _gameController = null;
    public FirstPersonController GameController
    {
        get
        {
            if (_gameController == null) TryGetComponent(out _gameController);
            return _gameController;
        }
    }

    /// <summary>
    /// 让玩家跳跃
    /// </summary>
    public abstract void DoJump();

    /// <summary>
    /// 让玩家滑铲
    /// </summary>
    public abstract void DoSlide();

    /// <summary>
    /// 开启玩家下坠模式
    /// </summary>
    public abstract void DoDrop();

    /// <summary>
    /// 开启玩家滑翔模式
    /// </summary>
    public abstract void DoGliding();

    /// <summary>
    /// 强制玩家至少停一帧。
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// 玩家进入平台时处理纵向弹跳
    /// </summary>
    /// <param name="force">向上力</param>
    public abstract void PlatformJump(float force);

    /// <summary>
    /// 更新鼠标视角移动
    /// 可手动调用更新。
    /// </summary>
    public abstract void UpdateMouseLook();

    /// <summary>
    /// 播放一次脚步声
    /// 播放的音效应与玩家所在的地面材质对应
    /// 即玩家当前踩到的地面
    /// </summary>
    public abstract void PlayFootStepSound();

    /// <summary>
    /// 获取 MouseLook 控制器模块
    /// </summary>
    /// <returns></returns>
    public abstract MouseLookBase GetMouseLook();

    /// <summary>
    /// 获取脚步控制器模块
    /// </summary>
    /// <returns></returns>
    public abstract Footstep GetFootStep();

    /// <summary>
    /// 返回玩家在指定状态下的最高速度
    /// </summary>
    /// <param name="playerState"></param>
    /// <returns></returns>
    public abstract float GetSpeedOnState(PlayerState playerState, bool includeModifiers = false);

    /// <summary>
    /// 返回玩家当前速度
    /// </summary>
    /// <returns></returns>
    public abstract float GetCurrentSpeed();

    public abstract PlayerRunToAimBehave GetRunToAimBehave();

    public abstract float GetHeadBobMagnitudes(bool verticalMagnitude);

    public abstract float GetSprintFov();
}