using UnityEngine;

public class PooledInstanceDisabler : MonoBehaviour
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
            transform.parent = ObjectRecyclerBase.Instance.transform;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}