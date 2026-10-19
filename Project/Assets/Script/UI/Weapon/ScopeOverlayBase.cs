using System;
using UnityEngine;

public abstract class ScopeOverlayBase : MonoBehaviour
{
    public abstract void SetActive(bool active);

    public abstract void SetupWeapon(SniperScopeOverlayBase weapon);

    /// <summary>
    /// 淡入淡出显示/隐藏瞄准镜 UI
    /// </summary>
    /// <param name="fadeIn"></param>
    /// <param name="speed"></param>
    /// <param name="delay"></param>
    /// <param name="onFinish"></param>
    /// <param name="onStart"></param>
    public abstract void Crossfade(bool fadeIn, float speed, float delay = 0, Action onFinish = null, Action onStart = null);

    private static ScopeOverlayBase _instance;
    public static ScopeOverlayBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScopeOverlayBase>();
            }
            return _instance;
        }
    }
}