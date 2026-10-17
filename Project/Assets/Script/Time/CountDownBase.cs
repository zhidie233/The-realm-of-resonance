using UnityEngine;

public abstract class CountDownBase : MonoBehaviour
{
    /// <summary>
    /// Is currently counting down?
    /// </summary>
    public virtual bool IsCounting
    {
        get;
        set;
    } = false;

    public abstract void StartCountDown(bool overrideIfStarted = false);

    public abstract void SetCount(int count);

    private static CountDownBase _instance;
    public static CountDownBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<CountDownBase>(); }
            return _instance;
        }
    }
}