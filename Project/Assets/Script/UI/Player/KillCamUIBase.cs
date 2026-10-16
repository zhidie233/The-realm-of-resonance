using UnityEngine;

public abstract class KillCamUIBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="gunID"></param>
    public abstract void Show(KillCamBase.KillCamInfo killCamInfo);

    /// <summary>
    /// 
    /// </summary>
    public abstract void Hide();


    private static KillCamUIBase _instance;
    public static KillCamUIBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<KillCamUIBase>();
            return _instance;
        }
    }
}