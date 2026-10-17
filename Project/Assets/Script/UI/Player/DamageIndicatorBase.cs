using UnityEngine;

public abstract class DamageIndicatorBase : MonoBehaviourBase
{
    public abstract void SetHit(Vector3 direction);

    private static DamageIndicatorBase _instance;

    public static DamageIndicatorBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DamageIndicatorBase>();
            }
            return _instance;
        }
    }
}