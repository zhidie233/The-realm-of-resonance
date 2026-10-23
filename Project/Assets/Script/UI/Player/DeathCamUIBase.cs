using UnityEngine;

public abstract class DeathCamUIBase : MonoBehaviour
{
    public abstract void Show(DeathCamBase.DeathCamInfo killCamInfo);

    public abstract void Hide();


    private static DeathCamUIBase _instance;
    public static DeathCamUIBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<DeathCamUIBase>();
            return _instance;
        }
    }
}