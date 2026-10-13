using UnityEngine;
using UnityEngine.Serialization;

public abstract class SniperScopeBase : MonoBehaviourBase
{
    /// <summary>
    /// 
    /// </summary>
    [FormerlySerializedAs("Scope"), SerializeField]
    private Sprite scopeTexture;
    public Sprite ScopeTexture
    {
        get => scopeTexture;
        set => scopeTexture = value;
    }
}