using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameModeSettings
{
    public string ModeName;
    public GameMode gameMode;
    [LovattoToogle] public bool isEnabled = true;

    [Header("Settings")]
    [LovattoToogle] public bool supportBots = false;
    [LovattoToogle] public bool AutoTeamSelection = false;
    [Range(1, 16)] public int RequiredPlayersToStart = 1;
    public OnRoundStartedSpawn onRoundStartedSpawn = OnRoundStartedSpawn.SpawnAfterSelectTeam;
    public OnPlayerDie onPlayerDie = OnPlayerDie.SpawnAfterDelay;
    public string GoalName = "Kills";

    [Header("Options")]
    public int[] maxPlayers = new int[] { 6, 2, 4, 8 };
    public int[] GameGoalsOptions = new int[] { 50, 100, 150, 200 };
    public int[] timeLimits = new int[] { 900, 600, 1200, 300 };

    public string GetGoalFullName(int goalID) { return string.Format("{0} {1}", GameGoalsOptions[goalID], GoalName); }

    /// <summary>
    /// 
    /// </summary>
    public string GetGoalName(int goalID)
    {
        if (GameGoalsOptions.Length <= 0) return GoalName;
        return $"{GameGoalsOptions[goalID]} {GoalName}";
    }

    public int GetGoalValue(int goalID)
    {
        if (GameGoalsOptions.Length <= 0) return 0;
        if (goalID >= GameGoalsOptions.Length) return GameGoalsOptions[GameGoalsOptions.Length - 1];

        return GameGoalsOptions[goalID];
    }

    [System.Serializable]
    public enum OnRoundStartedSpawn
    {
        SpawnAfterSelectTeam,
        WaitUntilRoundFinish,
    }

    [System.Serializable]
    public enum OnPlayerDie
    {
        SpawnAfterDelay,
        SpawnAfterRoundFinish,
    }
}