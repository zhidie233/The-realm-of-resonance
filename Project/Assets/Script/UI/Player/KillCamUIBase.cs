using UnityEngine;

public abstract class KillCamUIBase : MonoBehaviour
{
    public abstract void Show(KillCamBase.KillCamInfo killCamInfo);

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