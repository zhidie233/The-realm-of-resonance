using UnityEngine;

public abstract class ImpactMarkManagerBase : MonoBehaviour
{
    public static void InstantiateDecal(RaycastHit raycastHit)
    {
        if (Instance == null) return;

        Instance.InstanceDecal(raycastHit);
    }

    public abstract void InstanceDecal(RaycastHit raycastHit);

    private static ImpactMarkManagerBase _instance;
    public static ImpactMarkManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<ImpactMarkManagerBase>(); }
            return _instance;
        }
    }
}