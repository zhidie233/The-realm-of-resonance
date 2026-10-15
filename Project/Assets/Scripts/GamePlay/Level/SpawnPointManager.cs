﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Internal.BaseClass;

public class SpawnPointManager : MonoBehaviour
{
    public SpawnMode spawnMode = SpawnMode.Random;
    [GameFwkToogle] public bool drawSpawnPoints = true;

    [Header("References")]
    public KillCamBase killCameraInstance;
#if UNITY_EDITOR
    public Mesh SpawnPointPlayerGizmo;
#endif

    private List<SpawnPointBase> spawnPoints = new List<SpawnPointBase>();
    private int currentSpawnpoint = -1;

    /// <summary>
    /// 
    /// </summary>
    public static void AddSpawnPoint(SpawnPointBase point)
    {
        if (Instance == null) return;

        if (Instance.spawnPoints.Contains(point)) return;
        Instance.spawnPoints.Add(point);
    }

    /// <summary>
    /// Get the position and rotation to instance the player from one of the team spawn points in the scene
    /// </summary>
    public void GetPlayerSpawnPosition(Team team, out Vector3 position, out Quaternion rotation)
    {
        var point = GetSpawnPointForTeam(team);
        if (point == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            Debug.LogWarning($"Couldn't found spawnpoint for team {team.ToString()} in this scene.");
            return;
        }

        point.GetSpawnPosition(out position, out rotation);
    }

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the default selector mode. 
    /// </summary>
    /// <returns></returns>
    public SpawnPointBase GetSpawnPointForTeam(Team team) => GetSpawnPointForTeam(team, spawnMode);

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the given selector mode. 
    /// </summary>
    /// <returns></returns>
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
                Debug.LogWarning("There's not spawnpoints in this scene.");
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

    /// <summary>
    /// 
    /// </summary>
    public SpawnPointBase GetRandomSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if(teamPoints.Count <= 0) return null;

        return teamPoints[Random.Range(0, teamPoints.Count)];
    }

    /// <summary>
    /// 
    /// </summary>
    public SpawnPointBase GetSequentialSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if (teamPoints.Count <= 0) return null;

        currentSpawnpoint = (currentSpawnpoint + 1) % teamPoints.Count;
        return teamPoints[currentSpawnpoint];
    }

    /// <summary>
    /// Get the list of all the spawnpoints available for the given team
    /// </summary>
    public List<SpawnPointBase> GetListOfPointsForTeam(Team team)
    {
        if (team == Team.None) team = Team.All;

        var teamPoints = spawnPoints.FindAll(x => x.team == team);
        if (teamPoints.Count <= 0)
        {
            Debug.LogWarning("There's not spawnpoints for the team: " + team.GetTeamName());
            return null;
        }
        return teamPoints;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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