//#define LOADING_SCREEN
using Game.Internal.BaseClass;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Default Game scene loader
/// Use the default Unity Scene load method
/// If you want to use your custom scene loader e.g: Game's Loading Screen
/// Create a new script and inherited from <see cref="SceneLoaderBase"/>
/// </summary>
[CreateAssetMenu(fileName = "Simple Scene Loader", menuName = "Game/Level/Simple Loader")]
public class SimpleSceneLoader : SceneLoaderBase
{

    public override void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
#if !LOADING_SCREEN
        SceneManager.LoadScene(sceneName, loadSceneMode);
#else
        // IF YOU WANT TO USE AN EXTERNAL LOADING SCREEN
        // UNCOMMENT THE FIRST LINE OF THIS SCRIPT
        SceneLoaderManager.LoadScene(sceneName);
#endif
    }

    public override void LoadScene(int sceneID, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
#if !LOADING_SCREEN
        SceneManager.LoadScene(sceneID, loadSceneMode);
#else
        // IF YOU WANT TO USE AN EXTERNAL LOADING SCREEN
        // UNCOMMENT THE FIRST LINE OF THIS SCRIPT
        SceneLoaderManager.LoadSceneByBuildIndex(sceneID);
#endif
    }
}