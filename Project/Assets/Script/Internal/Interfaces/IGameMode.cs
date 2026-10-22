using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public interface IGameMode 
{
    /// 初始化游戏模式
    void Initialize();
    /// 回合时间归零时
    void OnFinishTime(bool gameOver);
    /// 本地玩家在此游戏模式中得分时调用
    void OnLocalPoint(int points, Team teamToAddPoint);
    /// 本地玩家击杀他人（仅本地收到）
    void OnLocalPlayerKill();
    /// 本地玩家死亡（仅本地收到）
    void OnLocalPlayerDeath();
    /// 新玩家进入房间时
    void OnOtherPlayerEnter(Player newPlayer);
    /// 玩家离开房间时
    void OnOtherPlayerLeave(Player otherPlayer);
    /// 房间属性更新
    void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged);
    /// 用于判断本地玩家是否在此游戏模式中获胜
    bool isLocalPlayerWinner { get; }
}
