using UnityEngine;
using System.Collections;
using Resonance.Foundation;
using Resonance.Core;
namespace Resonance.World
{

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
            EventBus.Publish(new AirSupplyRequestedEvent(transform.position, PackID));
            if(DestroyEffect != null)
            {
                Instantiate(DestroyEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
