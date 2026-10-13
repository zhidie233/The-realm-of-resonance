using UnityEngine;

namespace GFWK.Runtime.UI
{
    public abstract class LobbyLoadingScreenBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract LobbyLoadingScreenBase SetActive(bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public abstract LobbyLoadingScreenBase SetText(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
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