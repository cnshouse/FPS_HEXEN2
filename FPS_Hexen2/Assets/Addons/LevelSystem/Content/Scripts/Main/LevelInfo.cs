using MFPSEditor;
using System;
using UnityEngine;

namespace MFPS.Addon.LevelManager
{
    [System.Serializable]
    public class LevelInfo
    {
        public string Name = "Level";
        public int ScoreNeeded = 0;
        [SpritePreview(50)] public Sprite Icon;
        [HideInInspector] public int LevelID;
    }
}