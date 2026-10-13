using GFWK.Internal.Structures;
using UnityEngine;

public abstract class KillFeedUIBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="feed"></param>
    public abstract void SetKillFeed(KillFeed feed);

    /// <summary>
    /// 
    /// </summary>
    private static KillFeedUIBase _instance;
    public static KillFeedUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<KillFeedUIBase>(); }
            return _instance;
        }
    }
}