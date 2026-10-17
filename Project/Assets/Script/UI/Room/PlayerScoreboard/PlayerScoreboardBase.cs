using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerScoreboardBase : MonoBehaviourBase
{
    /// <summary>
    /// If you don't want to show the scoreboard, simply set this property to false
    /// </summary>
    private bool blockScoreboards = false;
    public bool BlockScoreboards
    {
        get { return blockScoreboards; }
        set
        {
            if (value == true)
            {
                SetActive(false);
            }
            blockScoreboards = value;
        }
    }
    public abstract void ForceUpdateAll();

    public abstract void SetActive(bool active, bool enableJoinButtons = false);

    public abstract void SetActiveByTeamMode(bool enableJoinButtons = false);

    public abstract void SetActiveByGameState(bool active);

    public abstract void SetActiveJoinButtons(bool active);

    public abstract void OnPlayerClicked(Player player);

    public abstract void ResetJoinButtons();

    public abstract bool RemoveUIBinding(PlayerScoreboardUIBase uiBinding);

    private static PlayerScoreboardBase _instance;
    public static PlayerScoreboardBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<PlayerScoreboardBase>(); }
            if (_instance == null) { _instance = UIReferences.Instance.playerScoreboards; }
            return _instance;
        }
    }
}