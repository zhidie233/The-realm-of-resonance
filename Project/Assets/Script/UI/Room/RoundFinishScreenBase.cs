using UnityEngine;

public abstract class RoundFinishScreenBase : MonoBehaviour
{
    public abstract void Show(string winner);

    public abstract void Hide();

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