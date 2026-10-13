using UnityEngine;
using UnityEngine.SceneManagement;

namespace GFWK.Internal.BaseClass
{
    public abstract class SceneLoaderBase : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        /// <summary>
        /// 
        /// </summary>
        public abstract void LoadScene(int sceneID, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        /// <summary>
        /// 
        /// </summary>
        private static SceneLoaderBase _instance;
        public static SceneLoaderBase Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<SceneLoaderBase>(); }
                return _instance;
            }
        }
    }
}