using UnityEngine;

/// <summary>
/// 名称牌绘制器的基类
/// 自定义脚本继承此类，自行处理游戏中玩家名称的绘制。
/// </summary>
public abstract class NamePlateBase : MonoBehaviourBase
{

    public static bool BlockDraw = false;

    public string PlayerName { get; set; }

    /// <summary>
    /// 设置要绘制的名称
    /// </summary>
    public abstract void SetName(string playerName);

    public abstract void SetActive(bool active);
}
