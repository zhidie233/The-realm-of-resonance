using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Internal.BaseClass
{
    public abstract class SceneLoaderBase : ScriptableObject
    {
        public abstract void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        public abstract void LoadScene(int sceneID, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

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