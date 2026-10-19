using UnityEngine;

public abstract class LootCasterBase : MonoBehaviour
{
    public struct LootData
    {
        public int KitID;
        public float Delay;
    }

    public abstract void SetUp(LootData dropData);
}