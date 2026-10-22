using UnityEngine;
/// 出生点基类
/// 你的自定义出生点脚本可从此类继承。
public abstract class SpawnPointBase : MonoBehaviour
{
    public Team team = Team.All;

    public static implicit operator Transform(SpawnPointBase d) => d.transform;
    /// 注册此出生点
    public virtual void Initialize()
    {
        SpawnPointManager.AddSpawnPoint(this);
    }
    /// 返回符合此出生点设置的出生位置
    public abstract void GetSpawnPosition(out Vector3 position, out Quaternion Rotation);

}
