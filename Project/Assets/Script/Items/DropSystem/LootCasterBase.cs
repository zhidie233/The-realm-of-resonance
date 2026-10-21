using UnityEngine;

public abstract class LootCasterBase : MonoBehaviour
{
    public struct LootData
    {
        public int PackID;
        public float Delay;
    }

    public abstract void SetUp(LootData dropData);
}