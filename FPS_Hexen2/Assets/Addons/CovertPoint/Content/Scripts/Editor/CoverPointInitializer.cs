﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using MFPSEditor;

public class CoverPointInitializer : MonoBehaviour
{
    private const string DEFINE_KEY = "CP";

    static CoverPointInitializer()
    {
        int start = PlayerPrefs.GetInt("mfps.addons.define." + DEFINE_KEY, 0);
        if (start == 0)
        {
            bool defines = EditorUtils.CompilerIsDefine(DEFINE_KEY);
            if (!defines)
            {
                EditorUtils.SetEnabled(DEFINE_KEY, true);
            }
            PlayerPrefs.SetInt("mfps.addons.define." + DEFINE_KEY, 1);
        }
    }

    [MenuItem("MFPS/Addons/CovertPoint/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }

    [MenuItem("MFPS/Addons/CovertPoint/Enable", true)]
    private static bool EnableValidate()
    {
        return !EditorUtils.CompilerIsDefine(DEFINE_KEY);
    }

    [MenuItem("MFPS/Addons/CovertPoint/Disable")]
    private static void Disable()
    {
        if (bl_GameData.Instance.gameModes.Exists(x => x.gameMode == GameMode.CP))
        {
            int id = bl_GameData.Instance.gameModes.FindIndex(x => x.gameMode == GameMode.CP);
            bl_GameData.Instance.gameModes[id].isEnabled = false;
            EditorUtility.SetDirty(bl_GameData.Instance);
            AssetDatabase.SaveAssets();
        }
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }

    [MenuItem("MFPS/Addons/CovertPoint/Disable", true)]
    private static bool DisableValidate()
    {
        return EditorUtils.CompilerIsDefine(DEFINE_KEY);
    }

    [MenuItem("MFPS/Addons/CovertPoint/Integrate")]
    private static void Instegrate()
    {
        bl_RoomSettings lb = FindObjectOfType<bl_RoomSettings>();
        if (lb != null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/CovertPoint/Content/Prefabs/CovertPoint.prefab", typeof(GameObject)) as GameObject;
            if (prefab != null)
            {
                GameObject g = PrefabUtility.InstantiatePrefab(prefab, EditorSceneManager.GetActiveScene()) as GameObject;
                GameObject gmo = GameObject.Find("GameModes");
                if (gmo != null)
                {
                    g.transform.parent = gmo.transform;
                    g.transform.localPosition = Vector3.zero;
                }
                g.transform.SetAsLastSibling();
                if (!bl_GameData.Instance.gameModes.Exists(x => x.gameMode == GameMode.CP))
                {
                    GameModeSettings gmi = new GameModeSettings();
                    gmi.gameMode = GameMode.CP;
                    gmi.ModeName = "Covert Point";
                    gmi.isEnabled = true;
                    gmi.RequiredPlayersToStart = 2;
                    bl_GameData.Instance.gameModes.Add(gmi);
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    int id = bl_GameData.Instance.gameModes.FindIndex(x => x.gameMode == GameMode.CP);
                    bl_GameData.Instance.gameModes[id].isEnabled = true;
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    AssetDatabase.SaveAssets();
                }
                EditorUtility.SetDirty(g);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                EditorGUIUtility.PingObject(g);
                var view = (SceneView)SceneView.sceneViews[0];
                view.ShowNotification(new GUIContent("Cover Point integrate in this map!"));
                Debug.Log("<color=green><b>Cover Point</b> integrate in this map!</color>");
            }
            else
            {
                Debug.Log("Can't found prefab!");
            }
        }
    }

    [MenuItem("MFPS/Addons/CovertPoint/Integrate", true)]
    private static bool InstegrateValidate()
    {
        bl_CoverPoint km = GameObject.FindObjectOfType<bl_CoverPoint>();
        bl_GameManager gm = GameObject.FindObjectOfType<bl_GameManager>();
        return (km == null && gm != null);
    }
}