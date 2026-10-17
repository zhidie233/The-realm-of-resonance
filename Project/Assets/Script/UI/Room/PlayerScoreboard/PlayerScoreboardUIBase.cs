using Game.Runtime.AI;
using Photon.Realtime;
using UnityEngine;

public abstract class PlayerScoreboardUIBase : MonoBehaviour
{
    public GameBotProperties Bot 
    { 
        get; 
        set; 
    }

    public bool isBotBinding 
    { 
        get; 
        set; 
    } = false;

    public Player RealPlayer 
    { 
        get; 
        set; 
    }

    public abstract void Init(Player player, GameBotProperties bot = null);

    /// <summary>
    /// 记分板每次刷新时调用（记分板打开期间）
    /// </summary>
    /// <returns></returns>
    public abstract bool Refresh();

    public abstract bool UpdateBot();

    public abstract int GetScore();

    public abstract Team GetTeam();
}