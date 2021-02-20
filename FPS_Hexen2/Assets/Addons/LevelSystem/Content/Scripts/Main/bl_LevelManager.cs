using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using MFPS.Addon.LevelManager;

public class bl_LevelManager : ScriptableObject
{
    public List<LevelInfo> Levels = new List<LevelInfo>();
    [Header("Settings")]
    [LovattoToogle] public bool UpdateLevelsInMidGame = true;

    public bool isNewLevel { get; set; }
    private int LastLevel = 0;

    /// <summary>
    /// 
    /// </summary>
    public void Initialize()
    {
#if ULSP
        if(bl_DataBase.Instance != null)
        {
            bl_DataBase.OnUpdateData -= OnDataUpdate;
            bl_DataBase.OnUpdateData += OnDataUpdate;
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void GetInfo()
    {
        LastLevel = GetLevelID();
    }

    /// <summary>
    /// Get the level index by the give scored
    /// </summary>
    /// <returns></returns>
    public int GetLevelID(int score)
    {
        if (Levels == null || Levels.Count <= 0)
        {
            Debug.LogWarning("There is not levels setup yet!");
            return 0;
        }

        //check that the score is not mayor than the last level required score.
        if (score >= Levels[Levels.Count - 1].ScoreNeeded) { return Levels.Count - 1; } //max level

        for (int i = 0; i < Levels.Count; i++)
        {
            if (score >= Levels[i].ScoreNeeded) continue;
            if (score < Levels[i].ScoreNeeded)
            {
                int id = (i > 0) ? i - 1 : i;
                return id;
            }
        }
        return 0;
    }

    /// <summary>
    /// Calculate the level by the give score
    /// </summary>
    /// <returns></returns>
    public LevelInfo GetLevel(int score)
    {
        int levelID = GetLevelID(score);
        return Levels[levelID];
    }

    /// <summary>
    /// Get the next level by the give score
    /// </summary>
    /// <returns></returns>
    public LevelInfo GetNextLevel(int score)
    {
        int levelID = GetLevelID(score);
        if((levelID + 1) < Levels.Count)
        {
            levelID++;
        }
        return Levels[levelID];
    }

    /// <summary>
    /// Get the level of the local player
    /// </summary>
    /// <returns></returns>
    public LevelInfo GetLevel()
    {
        int score = 0;
        if (Levels == null || Levels.Count <= 0)
            return null;

#if ULSP
        bl_DataBase db = bl_DataBase.Instance;
        if (db != null)
        {
            score = db.LocalUser.Score;
        }
#else
        Debug.LogWarning("Level system need 'ULogin Pro' add-on to work");
#endif

        int levelID = GetLevelID(score);
        return Levels[levelID];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public LevelInfo GetPlayerLevelInfo(Player player)
    {
        int score = 0;
        if (Levels == null || Levels.Count <= 0)
            return null;

#if ULSP
        if (player.CustomProperties.ContainsKey("TotalScore"))
            score = (int)player.CustomProperties["TotalScore"];
#else
        Debug.LogWarning("Level system need 'ULogin Pro' add-on to work");
#endif
        if (UpdateLevelsInMidGame)
        {
            score += player.GetPlayerScore();
        }
        if (score >= Levels[Levels.Count - 1].ScoreNeeded) { return Levels[Levels.Count - 1]; } //max level

        for (int i = 0; i < Levels.Count; i++)
        {
            if (score >= Levels[i].ScoreNeeded) continue;
            if (score < Levels[i].ScoreNeeded)
            {
                int id = (i > 0) ? i - 1 : i;
                return Levels[id];
            }
        }
        return Levels[0];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetLevelID()
    {
        int score = 0;
        if (Levels == null || Levels.Count <= 0)
            return 0;
#if ULSP
        bl_DataBase db = bl_DataBase.Instance;
        if (db != null)
        {
            score = db.LocalUser.Score;
        }
#else
        Debug.LogWarning("Level system need 'ULogin Pro' add-on to work");
#endif
        int levelID = GetLevelID(score);
        return levelID;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetSavedScore()
    {
#if ULSP
        bl_DataBase db = bl_DataBase.Instance;
        if (db != null)
        {
            return db.LocalUser.Score;
        }
        return 0;
#else
       return 0;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetRuntimeLocalScore()
    {
        int total = GetSavedScore() + PhotonNetwork.LocalPlayer.GetPlayerScore();
        return total;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetRuntimeLevelID()
    {
        int score = GetRuntimeLocalScore();
        int levelID = GetLevelID(score);
        return levelID;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="score"></param>
    public void Check(int score)
    {
        int current = GetLevelID(score);
        if (current > LastLevel)
        {
            isNewLevel = true;
        }
    }

#if ULSP
    /// <summary>
    /// 
    /// </summary>
    void OnDataUpdate(MFPS.ULogin.LoginUserInfo info)
    {
        Check(bl_DataBase.LocalLoggedUser.Score);
    }
#endif
    public void Refresh()
    {
        isNewLevel = false;
    }

#if UNITY_EDITOR
    [ContextMenu("Calculate")]
    void DoCal()
    {
        int addPerWave = 200;
        int currentWaveScore = addPerWave;
        int currentWave = 0;
        int currentScore = 0;
        List<LevelInfo> all = new List<LevelInfo>();

        for (int i = 0; i < 78; i++)
        {
            currentWave = (currentWave + 1) % 5;
            if(currentWave == 0)
            {
                currentWaveScore += addPerWave;
            }
            Sprite sp = UnityEditor.AssetDatabase.LoadAssetAtPath($"Assets/Addons/LevelSystem/Content/Art/Ranks/rank0{i.ToString("00")}.png", typeof(Sprite)) as Sprite;
            LevelInfo li = new LevelInfo()
            {
                Name = $"Level {i + 1}",
                ScoreNeeded = currentScore,
                Icon = sp
            };
            all.Add(li);
            currentScore += currentWaveScore;
        }
        Levels = all;
        UnityEditor.EditorUtility.SetDirty(Instance);
    }

    private void OnValidate()
    {
        for (int i = 0; i < Levels.Count; i++)
        {
            Levels[i].LevelID = i + 1;
        }
    }
#endif

    private static bl_LevelManager _instance;
    public static bl_LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<bl_LevelManager>("LevelManager") as bl_LevelManager;
            }
            return _instance;
        }
    }
}