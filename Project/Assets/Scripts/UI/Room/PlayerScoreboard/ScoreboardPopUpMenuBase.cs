using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class ScoreboardPopUpMenuBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public struct MenuFilter
    {
        public bool IsLocalPlayer;
        public bool IsBot;
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class MenuOptions
    {
        public string Title;
        public bool AllowedForLocal = false;
        public bool AllowedForBots = false;

        [HideInInspector] public Button OptionButton;
    }

    public static Action<int> onScoreboardMenuAction;
    public static Player TargetPlayer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    /// <returns></returns>
    public abstract ScoreboardPopUpMenuBase SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public abstract ScoreboardPopUpMenuBase SetTargetPlayer(Player player);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract ScoreboardPopUpMenuBase FilterMenuOptions(MenuFilter filter);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buttonName"></param>
    /// <returns>button id</returns>
    public abstract int AddButton(string buttonName, MenuFilter filter);
}