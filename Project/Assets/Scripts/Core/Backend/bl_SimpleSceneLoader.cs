//#define LOADING_SCREEN
using GFWK.Internal.BaseClass;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Default GFWK scene loader
/// Use the default Unity Scene load method
/// If you want to use your custom scene loader e.g: GFWorks Studio's Loading Screen
/// Create a new script and inherited from <see cref="bl_SceneLoaderBase"/>
/// </summary>
[CreateAssetMenu(fileName = "Simple Scene Loader", menuName = "Game Framework/Level/Simple Loader")]
public class bl_SimpleSceneLoader : bl_SceneLoaderBase
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneName"></param>
    public override void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
#if !LOADING_SCREEN
        SceneManager.LoadScene(sceneName, loadSceneMode);
#else
        // IF YOU WANT TO USE GFWORKS STUDIO'S LOADING SCREEN
        // UNCOMMENT THE FIRST LINE OF THIS SCRIPT
        bl_SceneLoaderManager.LoadScene(sceneName);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneID"></param>
    public override void LoadScene(int sceneID, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
#if !LOADING_SCREEN
        SceneManager.LoadScene(sceneID, loadSceneMode);
#else
        // IF YOU WANT TO USE GFWORKS STUDIO'S LOADING SCREEN
        // UNCOMMENT THE FIRST LINE OF THIS SCRIPT
        bl_SceneLoaderManager.LoadSceneByBuildIndex(sceneID);
#endif
    }
}