using UnityEngine;
using UnityEngine.SceneManagement;
/// 默认游戏场景加载器
/// 使用 Unity 默认的场景加载方式
/// 想用自己的场景加载器，例如游戏的加载界面
/// 新建一个脚本并继承 <see cref="SceneLoaderBase"/>
[CreateAssetMenu(fileName = "Simple Scene Loader", menuName = "项目/Level/Simple Loader")]
public class SimpleSceneLoader : SceneLoaderBase
{

    public override void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
#if !LOADING_SCREEN
        SceneManager.LoadScene(sceneName, loadSceneMode);
#else
        // 如果想使用外部加载界面
        // 取消本脚本第一行的注释
        SceneLoaderManager.LoadScene(sceneName);
#endif
    }

    public override void LoadScene(int sceneID, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
#if !LOADING_SCREEN
        SceneManager.LoadScene(sceneID, loadSceneMode);
#else
        // 如果想使用外部加载界面
        // 取消本脚本第一行的注释
        SceneLoaderManager.LoadSceneByBuildIndex(sceneID);
#endif
    }
}
