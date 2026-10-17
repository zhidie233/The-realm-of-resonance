using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public interface IGameMode 
{
    /// <summary>
    /// 初始化游戏模式
    /// </summary>
    void Initialize();
    /// <summary>
    /// 回合时间归零时
    /// </summary>
    void OnFinishTime(bool gameOver);
    /// <summary>
    /// 本地玩家在此游戏模式中得分时调用
    /// </summary>
    /// <param name="points"></param>
    void OnLocalPoint(int points, Team teamToAddPoint);
    /// <summary>
    /// 本地玩家击杀他人（仅本地收到）
    /// </summary>
    void OnLocalPlayerKill();
    /// <summary>
    /// 本地玩家死亡（仅本地收到）
    /// </summary>
    void OnLocalPlayerDeath();
    /// <summary>
    /// 新玩家进入房间时
    /// </summary>
    void OnOtherPlayerEnter(Player newPlayer);
    /// <summary>
    /// 玩家离开房间时
    /// </summary>
    void OnOtherPlayerLeave(Player otherPlayer);
    /// <summary>
    /// 房间属性更新
    /// </summary>
    void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged);
    /// <summary>
    /// 用于判断本地玩家是否在此游戏模式中获胜
    /// </summary>
    bool isLocalPlayerWinner { get; }
}