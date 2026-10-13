using UnityEngine;

public abstract class RoundFinishScreenBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="winner"></param>
    public abstract void Show(string winner);

    /// <summary>
    /// 
    /// </summary>
    public abstract void Hide();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public abstract void SetCountdown(int count);


    private static RoundFinishScreenBase _instance;
    public static RoundFinishScreenBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<RoundFinishScreenBase>();
            return _instance;
        }
    }
}