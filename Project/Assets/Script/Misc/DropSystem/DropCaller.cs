using UnityEngine;
using System.Collections;

public class DropCaller : DropCallerBase
{
    public GameObject DestroyEffect;
    private int KitID = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dropData"></param>
    public override void SetUp(DropData dropData)
    {
        KitID = dropData.KitID;
        StartCoroutine(CallProcess(dropData.Delay));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
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