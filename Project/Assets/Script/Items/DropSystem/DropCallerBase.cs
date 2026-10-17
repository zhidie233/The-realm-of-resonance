using UnityEngine;

public abstract class DropCallerBase : MonoBehaviour
{
    public struct DropData
    {
        public int KitID;
        public float Delay;
    }

    public abstract void SetUp(DropData dropData);
}