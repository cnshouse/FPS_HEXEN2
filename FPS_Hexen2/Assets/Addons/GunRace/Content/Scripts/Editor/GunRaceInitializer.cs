using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MFPSEditor;

public class GunRaceInitializer : MonoBehaviour
{

    private const string DEFINE_KEY = "GR";

    static GunRaceInitializer()
    {
        int start = PlayerPrefs.GetInt("mfps.addons.define." + DEFINE_KEY, 0);
        if (start == 0)
        {
            if (!EditorUtils.CompilerIsDefine(DEFINE_KEY))
            {
               EditorUtils.SetEnabled(DEFINE_KEY, true);
            }
            PlayerPrefs.SetInt("mfps.addons.define." + DEFINE_KEY, 1);
        }
    }


    [MenuItem("MFPS/Addons/GunRace/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }


    [MenuItem("MFPS/Addons/GunRace/Enable", true)]
    private static bool EnableValidate()
    {
        return !EditorUtils.CompilerIsDefine(DEFINE_KEY);
    }


    [MenuItem("MFPS/Addons/GunRace/Disable")]
    private static void Disable()
    {

        if (bl_GameData.Instance.gameModes.Exists(x => x.gameMode == GameMode.GR))
        {
            int id = bl_GameData.Instance.gameModes.FindIndex(x => x.gameMode == GameMode.GR);
            bl_GameData.Instance.gameModes[id].isEnabled = true;
            EditorUtility.SetDirty(bl_GameData.Instance);
            AssetDatabase.SaveAssets();
        }
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }


    [MenuItem("MFPS/Addons/GunRace/Disable", true)]
    private static bool DisableValidate()
    {
        return EditorUtils.CompilerIsDefine(DEFINE_KEY);
    }

    [MenuItem("MFPS/Addons/GunRace/Integrate")]
    private static void Instegrate()
    {
        bl_RoomSettings lb = FindObjectOfType<bl_RoomSettings>();
        if (lb != null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/GunRace/Content/Prefabs/GunRace.prefab", typeof(GameObject)) as GameObject;
            if (prefab != null)
            {
                GameObject g = PrefabUtility.InstantiatePrefab(prefab, UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()) as GameObject;
                GameObject gmo = GameObject.Find("GameModes");
                if(gmo != null)
                {
                    g.transform.parent = gmo.transform;
                }
                g.transform.SetAsLastSibling();
                if (!bl_GameData.Instance.gameModes.Exists(x => x.gameMode == GameMode.GR))
                {
                    GameModeSettings gmi = new GameModeSettings();
                    gmi.gameMode = GameMode.GR;
                    gmi.ModeName = "Gun Race";
                    gmi.isEnabled = true;
                    bl_GameData.Instance.gameModes.Add(gmi);
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    int id = bl_GameData.Instance.gameModes.FindIndex(x => x.gameMode == GameMode.GR);
                    bl_GameData.Instance.gameModes[id].isEnabled = true;
                    EditorUtility.SetDirty(bl_GameData.Instance);
                    AssetDatabase.SaveAssets();
                }
                EditorUtility.SetDirty(g);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());


                EditorGUIUtility.PingObject(g);
                var view = (SceneView)SceneView.sceneViews[0];
                view.ShowNotification(new GUIContent("Gun Race integrate in this map!"));
                Debug.Log("<color=green><b>Gun Race</b> integrate in this map!</color>");
            }
            else
            {
                Debug.Log("Can't found prefab!");
            }
        }
    }

    [MenuItem("MFPS/Addons/GunRace/Integrate", true)]
    private static bool InstegrateValidate()
    {
        bl_GunRace km = GameObject.FindObjectOfType<bl_GunRace>();
        bl_GameManager gm = GameObject.FindObjectOfType<bl_GameManager>();
        return (km == null && gm != null);
    }
}