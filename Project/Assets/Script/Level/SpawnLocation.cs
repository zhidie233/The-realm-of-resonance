using UnityEngine;
#if UNITY_EDITOR
#endif
/// 默认出生点实现
/// 若要使用自己的出生点系统，让脚本从 SpawnPointBase.cs 继承即可
public class SpawnLocation : SpawnPointBase
{

    [System.Serializable]
    public enum Shape
    {
        Cube,
        Dome
    }

    public Shape shape = Shape.Dome;
    public float SpawnSpace = 3f;
    [Tooltip("地面检测高度上限")]
    public float groundSnapLimit = 1.5f;
    [Tooltip("玩家出生时朝向出生方向还是随机方向")]
    [GameToggle] public bool randomRotation = false;

    RaycastHit hitInfo;
    private const float BASE_VERTICAL_THRESHOLD = 0.01f;

    void OnEnable()
    {
        // 确保把此出生点加入可用点列表
        // 否则你的出生点永远不会被使用。
        Initialize();
    }
    /// 返回符合此出生点设置的出生位置
    public override void GetSpawnPosition(out Vector3 position, out Quaternion Rotation)
    {
        GetSpawnPosition(SpawnSpace, out position, out Rotation);
    }
    /// 返回带指定偏移的出生位置
    public void GetSpawnPosition(float radiusSpace, out Vector3 position, out Quaternion Rotation)
    {
        position = GetPositionInsideShape(radiusSpace);
        position = SnapPositionToGround(position);

        if (randomRotation) Rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
        else Rotation = transform.rotation;
    }

    private Vector3 GetPositionInsideShape(float space)
    {
        Vector3 s = transform.position;
        if (shape == Shape.Dome)
        {
            s = Random.insideUnitSphere * space;
            s = transform.position + new Vector3(s.x, BASE_VERTICAL_THRESHOLD, s.z);
        }
        else if (shape == Shape.Cube)
        {
            Vector3 localSize = transform.InverseTransformDirection(transform.localScale * 0.45f);
            s.x += Random.Range(-localSize.x, localSize.x);
            s.z += Random.Range(-localSize.z, localSize.z);
        }

        return s;
    }

    public Vector3 SnapPositionToGround(Vector3 position)
    {
        if (Physics.SphereCast(new Ray(position + (Vector3.up * groundSnapLimit), Vector3.down), 0.1f, out hitInfo, groundSnapLimit * 2.0f, GameConfig.TagsAndLayerSettings.EnvironmentOnly))
        {
            if (hitInfo.collider != null) position.y = hitInfo.point.y + BASE_VERTICAL_THRESHOLD;
        }

        return position;
    }

#if UNITY_EDITOR
    DomeGizmo _gizmo = null;
    void OnDrawGizmosSelected()
    {
        Draw();
    }

    private void OnDrawGizmos()
    {
        if (SpawnPointManager.Instance == null || !SpawnPointManager.Instance.drawSpawnPoints) return;
        Draw();
    }

    void Draw()
    {
        if (SpawnPointManager.Instance == null || Application.isPlaying) return;

        if (shape == Shape.Dome)
        {
            float h = 180;
            if (_gizmo == null || _gizmo.horizon != h)
            {
                _gizmo = new DomeGizmo(h);
            }
        }

        Color c = (team == Team.Team2) ? GameConfig.Instance.Team2Color : GameConfig.Instance.Team1Color;
        if (team == Team.All) { c = Color.white; }
        Gizmos.color = c;

        if (shape == Shape.Dome)
        {
            _gizmo.Draw(transform, c, SpawnSpace);

        }
        else if (shape == Shape.Cube)
        {
            var matrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            c.a = 0.3f;
            Gizmos.color = c;
            Vector3 basePos = Vector3.zero;
            basePos.y += 1.1f;
            Gizmos.DrawCube(basePos, new Vector3(transform.localScale.x, 2.2f, transform.localScale.z));
            Gizmos.DrawWireCube(basePos, new Vector3(transform.localScale.x, 2.2f, transform.localScale.z));
            Gizmos.matrix = matrix;
        }

        if (SpawnPointManager.Instance.SpawnPointPlayerGizmo != null)
        {
            Gizmos.DrawWireMesh(SpawnPointManager.Instance.SpawnPointPlayerGizmo, transform.position, transform.rotation, Vector3.one * 2.75f);
        }
        if (!randomRotation)
            Gizmos.DrawLine(base.transform.position + ((base.transform.forward * this.SpawnSpace)), base.transform.position + (((base.transform.forward * 2f) * this.SpawnSpace)));
        Gizmos.color = Color.white;
    }
#endif
}
