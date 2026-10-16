using UnityEngine;

public class PoolObjectDeactivator : MonoBehaviour
{

    public bool Pooled = true;
    public float lifeTime = 5.0f;

    /// <summary>
    /// 
    /// </summary>
	void OnEnable()
    {
        Invoke(nameof(Disable), lifeTime);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        CancelInvoke();
    }

    /// <summary>
    /// 
    /// </summary>
    void Disable()
    {
        if (Pooled)
        {
            transform.parent = ObjectPoolingBase.Instance.transform;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}