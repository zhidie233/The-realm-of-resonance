using UnityEngine;

public class PoolObjectDeactivator : MonoBehaviour
{

    public bool Pooled = true;
    public float lifeTime = 5.0f;

	void OnEnable()
    {
        Invoke(nameof(Disable), lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

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