using UnityEngine;
using UnityEngine.Serialization;

public abstract class SniperScopeOverlayBase : MonoBehaviourBase
{
    [FormerlySerializedAs("Scope"), SerializeField]
    private Sprite scopeTexture;
    public Sprite ScopeTexture
    {
        get => scopeTexture;
        set => scopeTexture = value;
    }
}