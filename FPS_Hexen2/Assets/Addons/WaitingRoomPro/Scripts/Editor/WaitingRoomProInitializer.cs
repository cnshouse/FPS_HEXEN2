using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaitingRoomProInitializer : MonoBehaviour
{

    [MenuItem("MFPS/Addons/Waiting Room/Integrate")]
    private static void Instegrate()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/WaitingRoomPro/Prefabs/Waiting Room Pro.prefab", typeof(GameObject)) as GameObject;
        if (prefab != null)
        {
            bl_Lobby lb = FindObjectOfType<bl_Lobby>();
            if (lb != null)
            {
                GameObject newObj = PrefabUtility.InstantiatePrefab(prefab, UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()) as GameObject;
                bl_WaitingRoom wr = bl_Lobby.Instance.transform.GetComponentInChildren<bl_WaitingRoom>(true);
                if (wr != null)
                {
                    newObj.transform.SetParent(wr.transform.parent, false);
                    wr.gameObject.name += " [Default]";
                    wr.gameObject.SetActive(false);
                }
                else
                {
                    newObj.transform.SetParent(bl_LobbyUI.Instance.FadeAlpha.transform, false);
                }
                newObj.transform.SetSiblingIndex(7);
                EditorUtility.SetDirty(lb);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>Waiting Room Pro integrate!</color>");
            }
        }
        else
        {
            Debug.Log("Can't found prefab!");
        }
    }

    [MenuItem("MFPS/Addons/Waiting Room/Integrate", true)]
    private static bool InstegrateValidate()
    {
        bl_WaitingRoomPro km = GameObject.FindObjectOfType<bl_WaitingRoomPro>();
        bl_Lobby lb = FindObjectOfType<bl_Lobby>();
        return (km == null && lb != null);
    }

}