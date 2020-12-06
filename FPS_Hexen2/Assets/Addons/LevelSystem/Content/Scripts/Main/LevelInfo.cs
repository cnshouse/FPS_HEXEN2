using System;
using UnityEngine;

[System.Serializable]
public class LevelInfo
{
    public string Name = "Level";
    public int ScoreNeeded = 0;
    public Sprite Icon;
    [HideInInspector] public int LevelID;
}