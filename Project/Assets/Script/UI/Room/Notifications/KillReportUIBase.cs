using UnityEngine;

public abstract class KillReportUIBase : MonoBehaviour
{
    public abstract void SetKillFeed(KillReport feed);

    private static KillReportUIBase _instance;
    public static KillReportUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<KillReportUIBase>(); }
            return _instance;
        }
    }
}