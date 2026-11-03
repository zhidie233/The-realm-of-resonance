using UnityEngine;
using UnityEngine.SceneManagement;
/// 默认游戏场景加载器
/// 使用 Unity 默认的场景加载方式
[CreateAssetMenu(fileName = "Simple Scene Loader", menuName = "项目/Level/Simple Loader")]
public class SimpleSceneLoader : ScriptableObject
{
    private static SimpleSceneLoader _instance;
    public static SimpleSceneLoader Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<SimpleSceneLoader>(); }
            return _instance;
        }
    }

    public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(sceneName, loadSceneMode);
    }

    public void LoadScene(int sceneID, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(sceneID, loadSceneMode);
    }
}
