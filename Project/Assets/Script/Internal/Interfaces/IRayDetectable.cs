/// 用于标记可被本地玩家相机射线检测到的物体
public interface IRayDetectable
{
    /// 本地玩家在一定近距离注视此物品时调用
    void OnRayDetectedByPlayer();

    /// 本地玩家原本注视此物品、随后移开视线时调用。
    void OnUnDetectedByPlayer();
}
