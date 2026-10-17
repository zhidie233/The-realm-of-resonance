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
    /// Called each time the scoreboard is update (when the scoreboard is open)
    /// </summary>
    /// <returns></returns>
    public abstract bool Refresh();

    public abstract bool UpdateBot();

    public abstract int GetScore();

    public abstract Team GetTeam();
}