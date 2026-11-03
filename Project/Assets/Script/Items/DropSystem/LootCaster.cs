using UnityEngine;
using System.Collections;

public class LootCaster : MonoBehaviour
{
    public struct LootData
    {
        public int PackID;
        public float Delay;
    }

    public GameObject DestroyEffect;
    private int PackID = 0;

    public void SetUp(LootData dropData)
    {
        PackID = dropData.PackID;
        StartCoroutine(CallProcess(dropData.Delay));
    }

    IEnumerator CallProcess(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameEvents.DispatchDropEvent(transform.position, PackID);
        if(DestroyEffect != null)
        {
            Instantiate(DestroyEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}