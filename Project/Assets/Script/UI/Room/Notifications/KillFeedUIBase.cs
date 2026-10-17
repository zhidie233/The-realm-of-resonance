using Game.Internal.Structures;
using UnityEngine;

public abstract class KillFeedUIBase : MonoBehaviour
{
    public abstract void SetKillFeed(KillFeed feed);

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