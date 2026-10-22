using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class ScoreboardPopUpMenuBase : MonoBehaviour
{
    public struct MenuFilter
    {
        public bool IsLocalPlayer;
        public bool IsBot;
    }

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

    public abstract ScoreboardPopUpMenuBase SetActive(bool active);

    public abstract ScoreboardPopUpMenuBase SetTargetPlayer(Player player);

    public abstract ScoreboardPopUpMenuBase FilterMenuOptions(MenuFilter filter);
    /// 
    /// <returns>按钮 id</returns>
    public abstract int AddButton(string buttonName, MenuFilter filter);
}
