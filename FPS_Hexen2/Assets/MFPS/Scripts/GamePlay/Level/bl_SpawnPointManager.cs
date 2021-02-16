using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_SpawnPointManager : MonoBehaviour
{
    public SpawnMode spawnMode = SpawnMode.Random;
    [LovattoToogle] public bool drawSpawnPoints = true;
#if UNITY_EDITOR
    public Mesh SpawnPointPlayerGizmo;
#endif

    private List<bl_SpawnPoint> spawnPoints = new List<bl_SpawnPoint>();
    private int currentSpawnpoint = -1;

    /// <summary>
    /// 
    /// </summary>
    public void AddSpawnPoint(bl_SpawnPoint point)
    {
        if (spawnPoints.Contains(point)) return;
        spawnPoints.Add(point);
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
            return;
        }

        point.GetSpawnPosition(out position, out rotation);
    }

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the default selector mode. 
    /// </summary>
    /// <returns></returns>
    public bl_SpawnPoint GetSpawnPointForTeam(Team team) => GetSpawnPointForTeam(team, spawnMode);

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the given selector mode. 
    /// </summary>
    /// <returns></returns>
    public bl_SpawnPoint GetSpawnPointForTeam(Team team, SpawnMode m_spawnMode)
    {
        if(spawnPoints.Count <= 0)
        {
            Debug.LogWarning("There's not spawnpoints in this scene.");
            return null;
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
    public bl_SpawnPoint GetRandomSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if(teamPoints.Count <= 0) return null;

        return teamPoints[Random.Range(0, teamPoints.Count)];
    }

    /// <summary>
    /// 
    /// </summary>
    public bl_SpawnPoint GetSequentialSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if (teamPoints.Count <= 0) return null;

        currentSpawnpoint = (currentSpawnpoint + 1) % teamPoints.Count;
        return teamPoints[currentSpawnpoint];
    }

    /// <summary>
    /// Get the list of all the spawnpoints available for the given team
    /// </summary>
    public List<bl_SpawnPoint> GetListOfPointsForTeam(Team team)
    {
        if (team == Team.None) team = Team.All;

        var teamPoints = spawnPoints.FindAll(x => x.m_Team == team);
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
    public bl_SpawnPoint GetSingleRandom() => spawnPoints[Random.Range(0, spawnPoints.Count)];

    [System.Serializable]
    public enum SpawnMode
    {
        Random = 1,
        Sequential = 2,
    }

    private static bl_SpawnPointManager _instance;
    public static bl_SpawnPointManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_SpawnPointManager>();
            }
            return _instance;
        }
    }
}