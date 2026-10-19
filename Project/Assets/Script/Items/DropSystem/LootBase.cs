using UnityEngine;

/// <summary>
/// 游戏内空投/包裹的基类
/// 自定义空投/包裹投放脚本请从此类继承
/// </summary>
public abstract class LootBase : MonoBehaviour
{
    public struct LootData
    {
        public int KitID;
        public Vector3 DropPosition;
        public float DeliveryDuration;
        public GameObject DropPrefab;
    }

    /// <summary>
    /// 处理空投/包裹实例化的函数
    /// </summary>
    public abstract void Dispatch(LootData dropData);
}