using UnityEngine;
using System.Collections;

public class LootCaster : LootCasterBase
{
    public GameObject DestroyEffect;
    private int KitID = 0;

    public override void SetUp(LootData dropData)
    {
        KitID = dropData.KitID;
        StartCoroutine(CallProcess(dropData.Delay));
    }

    IEnumerator CallProcess(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameEvents.DispatchDropEvent(transform.position, KitID);
        if(DestroyEffect != null)
        {
            Instantiate(DestroyEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}