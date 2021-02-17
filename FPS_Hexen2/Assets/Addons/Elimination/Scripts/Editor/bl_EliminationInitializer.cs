using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MFPSEditor;

public class bl_EliminationInitializer : MonoBehaviour
{
    private const string DEFINE_KEY = "ELIM";

    [MenuItem("MFPS/Addons/Elimination/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }

    [MenuItem("MFPS/Addons/Elimination/Enable", true)]
    private static bool EnableValidate()
    {
        return !EditorUtils.CompilerIsDefine(DEFINE_KEY);
    }

    [MenuItem("MFPS/Addons/Elimination/Disable")]
    private static void Disable()
    {
        if (bl_GameData.Instance.gameModes.Exists(x => x.gameMode == GameMode.ELIM))
        {
            int id = bl_GameData.Instance.gameModes.FindIndex(x => x.gameMode == GameMode.ELIM);
            bl_GameData.Instance.gameModes[id].isEnabled = false;
            EditorUtility.SetDirty(bl_GameData.Instance);
            AssetDatabase.SaveAssets();
        }
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }

    [MenuItem("MFPS/Addons/Elimination/Disable", true)]
    private static bool DisableValidate()
    {
        return EditorUtils.CompilerIsDefine(DEFINE_KEY);
    }

    [MenuItem("MFPS/Addons/Elimination/Integrate")]
    private static void Instegrate()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/Elimination/Prefabs/Elimination.prefab", typeof(GameObject)) as GameObject;
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
            if (!bl_GameData.Instance.gameModes.Exists(x => x.gameMode == GameMode.ELIM))
            {
                GameModeSettings gmi = new GameModeSettings();
                gmi.gameMode = GameMode.ELIM;
                gmi.ModeName = "Elimination";
                gmi.isEnabled = true;
                gmi.RequiredPlayersToStart = 2;
                gmi.AutoTeamSelection = true;
                gmi.onRoundStartedSpawn = GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish;
                gmi.onPlayerDie = GameModeSettings.OnPlayerDie.SpawnAfterRoundFinish;
                bl_GameData.Instance.gameModes.Add(gmi);
                EditorUtility.SetDirty(bl_GameData.Instance);
                AssetDatabase.SaveAssets();
            }
            else
            {
                int id = bl_GameData.Instance.gameModes.FindIndex(x => x.gameMode == GameMode.ELIM);
                bl_GameData.Instance.gameModes[id].isEnabled = true;
                EditorUtility.SetDirty(bl_GameData.Instance);
                AssetDatabase.SaveAssets();
            }

            EditorUtility.SetDirty(g);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorGUIUtility.PingObject(g);
            var view = (SceneView)SceneView.sceneViews[0];
            view.ShowNotification(new GUIContent("Elimination integrate in this map!"));
            Debug.Log("<color=green><b>Elimination</b> integrate in this map!</color>");
        }
        else
        {
            Debug.Log("Can't found prefab!");
        }
    }

    [MenuItem("MFPS/Addons/Elimination/Integrate", true)]
    private static bool InstegrateValidate()
    {
        bl_Elimination km = GameObject.FindObjectOfType<bl_Elimination>();
        bl_GameManager gm = GameObject.FindObjectOfType<bl_GameManager>();
        return (km == null && gm != null);
    }
}