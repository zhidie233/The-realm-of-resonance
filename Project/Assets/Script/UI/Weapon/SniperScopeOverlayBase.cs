using UnityEngine;

public abstract class SniperScopeOverlayBase : MonoBehaviourBase
{
    [SerializeField]
    private Sprite scopeTexture;
    public Sprite ScopeTexture
    {
        get => scopeTexture;
        set => scopeTexture = value;
    }
}