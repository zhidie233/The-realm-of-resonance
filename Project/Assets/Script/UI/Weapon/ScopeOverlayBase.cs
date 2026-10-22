using System;
using UnityEngine;

public abstract class ScopeOverlayBase : MonoBehaviour
{
    public abstract void SetActive(bool active);

    public abstract void SetupWeapon(SniperScopeOverlayBase weapon);
    /// 淡入淡出显示/隐藏瞄准镜 UI
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
