using UnityEngine;

/// 游戏内空投/包裹的基类
/// 自定义空投/包裹投放脚本请从此类继承
public abstract class LootBase : MonoBehaviour
{
    public struct LootData
    {
        public int PackID;
        public Vector3 DropPosition;
        public float DeliveryDuration;
        public GameObject DropPrefab;
    }

    /// 处理空投/包裹实例化的函数
    public abstract void Dispatch(LootData dropData);
}
