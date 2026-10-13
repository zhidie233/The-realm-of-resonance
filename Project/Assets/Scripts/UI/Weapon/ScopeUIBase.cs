using System;
using UnityEngine;

public abstract class ScopeUIBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    public abstract void SetupWeapon(SniperScopeBase weapon);

    /// <summary>
    /// Show/Hide the scope UI with a fade transition
    /// </summary>
    /// <param name="fadeIn"></param>
    /// <param name="speed"></param>
    /// <param name="delay"></param>
    /// <param name="onFinish"></param>
    /// <param name="onStart"></param>
    public abstract void Crossfade(bool fadeIn, float speed, float delay = 0, Action onFinish = null, Action onStart = null);

    /// <summary>
    /// 
    /// </summary>
    private static ScopeUIBase _instance;
    public static ScopeUIBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScopeUIBase>();
            }
            return _instance;
        }
    }
}