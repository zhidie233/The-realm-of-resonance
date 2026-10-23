using UnityEngine;

public abstract class BulletMarkManagerBase : MonoBehaviour
{
    public static void InstantiateDecal(RaycastHit raycastHit)
    {
        if (Instance == null) return;

        Instance.InstanceDecal(raycastHit);
    }

    public abstract void InstanceDecal(RaycastHit raycastHit);

    private static BulletMarkManagerBase _instance;
    public static BulletMarkManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<BulletMarkManagerBase>(); }
            return _instance;
        }
    }
}