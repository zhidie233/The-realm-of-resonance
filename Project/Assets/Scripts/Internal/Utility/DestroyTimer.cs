using UnityEngine;
using System.Collections;

public class DestroyTimer : MonoBehaviour
{

    public float destroyAfter = 15.0f;

    void Start()
    {
        if(destroyAfter > 0)
        Destroy(gameObject, destroyAfter);
    }

    public void Desactive()
    {
        gameObject.SetActive(false);
    }
}