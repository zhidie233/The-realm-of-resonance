using UnityEngine;

public abstract class BulletDecalManagerBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public static void InstantiateDecal(RaycastHit raycastHit)
    {
        if (Instance == null) return;

        Instance.InstanceDecal(raycastHit);
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void InstanceDecal(RaycastHit raycastHit);

    /// <summary>
    /// 
    /// </summary>
    private static BulletDecalManagerBase _instance;
    public static BulletDecalManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<BulletDecalManagerBase>(); }
            return _instance;
        }
    }
}