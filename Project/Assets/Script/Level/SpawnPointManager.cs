using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public SpawnMode spawnMode = SpawnMode.Random;
    [GameToggle] public bool drawSpawnPoints = true;

    [Header("References")]
    public DeathCamBase killCameraInstance;
#if UNITY_EDITOR
    public Mesh SpawnPointPlayerGizmo;
#endif

    private List<SpawnPointBase> spawnPoints = new List<SpawnPointBase>();
    private int currentSpawnpoint = -1;

    public static void AddSpawnPoint(SpawnPointBase point)
    {
        if (Instance == null) return;

        if (Instance.spawnPoints.Contains(point)) return;
        Instance.spawnPoints.Add(point);
    }
    /// 从场景中该队伍的某个出生点获取实例化玩家的位置和旋转
    public void GetPlayerSpawnPosition(Team team, out Vector3 position, out Quaternion rotation)
    {
        var point = GetSpawnPointForTeam(team);
        if (point == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            Debug.LogWarning($"场景中没有找到阵营 {team} 的出生点。");
            return;
        }

        point.GetSpawnPosition(out position, out rotation);
    }
    /// 按默认选择模式从所有已注册出生点中获取一个出生点。
    public SpawnPointBase GetSpawnPointForTeam(Team team) => GetSpawnPointForTeam(team, spawnMode);
    /// 按指定选择模式从所有已注册出生点中获取一个出生点。
    public SpawnPointBase GetSpawnPointForTeam(Team team, SpawnMode m_spawnMode)
    {
        if(spawnPoints.Count <= 0)
        {
            var all = FindObjectsOfType<SpawnPointBase>();
            if (all.Length > 0)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    all[i].Initialize();
                }
            }
            else
            {
                Debug.LogWarning("场景中没有出生点。");
                return null;
            }
        }

        switch(m_spawnMode)
        {
            case SpawnMode.Random:
            default:
                return GetRandomSpawnPoint(team);
            case SpawnMode.Sequential:
                return GetSequentialSpawnPoint(team);
        }
    }

    public SpawnPointBase GetRandomSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if(teamPoints.Count <= 0) return null;

        return teamPoints[Random.Range(0, teamPoints.Count)];
    }

    public SpawnPointBase GetSequentialSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if (teamPoints.Count <= 0) return null;

        currentSpawnpoint = (currentSpawnpoint + 1) % teamPoints.Count;
        return teamPoints[currentSpawnpoint];
    }
    /// 获取指定队伍可用的所有出生点列表
    public List<SpawnPointBase> GetListOfPointsForTeam(Team team)
    {
        if (team == Team.None) team = Team.All;

        var teamPoints = spawnPoints.FindAll(x => x.team == team);
        if (teamPoints.Count <= 0)
        {
            Debug.LogWarning("该阵营没有出生点：" + team.GetTeamName());
            return null;
        }
        return teamPoints;
    }

    public SpawnPointBase GetSingleRandom() => spawnPoints[Random.Range(0, spawnPoints.Count)];

    [System.Serializable]
    public enum SpawnMode
    {
        Random = 1,
        Sequential = 2,
    }

    private static SpawnPointManager _instance;
    public static SpawnPointManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SpawnPointManager>();
            }
            return _instance;
        }
    }
}
