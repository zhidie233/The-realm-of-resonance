using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 出生点基类
/// 你的自定义出生点脚本可从此类继承。
/// </summary>
public abstract class SpawnPointBase : MonoBehaviour
{
    [FormerlySerializedAs("m_Team")]
    public Team team = Team.All;

    public static implicit operator Transform(SpawnPointBase d) => d.transform;

    /// <summary>
    /// 注册此出生点
    /// </summary>
    public virtual void Initialize()
    {
        SpawnPointManager.AddSpawnPoint(this);
    }

    /// <summary>
    /// 返回符合此出生点设置的出生位置
    /// </summary>
    public abstract void GetSpawnPosition(out Vector3 position, out Quaternion Rotation);

}