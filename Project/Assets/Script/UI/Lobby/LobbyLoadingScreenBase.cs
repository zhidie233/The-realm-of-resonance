using UnityEngine;

namespace Game.Runtime.UI
{
    public abstract class LobbyLoadingScreenBase : MonoBehaviour
    {
        public abstract LobbyLoadingScreenBase SetActive(bool active);

        public abstract LobbyLoadingScreenBase SetText(string text);

        public abstract LobbyLoadingScreenBase HideIn(float delay, bool faded);


        private static LobbyLoadingScreenBase _instance;
        public static LobbyLoadingScreenBase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LobbyLoadingScreenBase>();
                }
                return _instance;
            }
        }
    }
}