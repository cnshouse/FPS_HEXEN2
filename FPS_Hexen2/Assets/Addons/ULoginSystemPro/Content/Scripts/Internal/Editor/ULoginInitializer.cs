using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using MFPSEditor;

public class ULoginInitializer : MonoBehaviour
{
    private const string DEFINE_KEY = "ULSP";

#if !ULSP
    [MenuItem("MFPS/Addons/ULogin/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif
#if ULSP
    [MenuItem("MFPS/Addons/ULogin/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/ULogin/Integrate")]
    private static void Instegrate()
    {
        if (SceneManager.sceneCountInBuildSettings > 0)
        {
            Scene menuScene = EditorSceneManager.OpenScene("Assets/MFPS/Scenes/MainMenu.unity", OpenSceneMode.Single);
            GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/ULoginSystemPro/Content/Prefabs/UI/Profile [MFPS].prefab", typeof(GameObject)) as GameObject;
            GameObject rankingprefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/ULoginSystemPro/Content/Prefabs/UI/Ranking [MFPS].prefab", typeof(GameObject)) as GameObject;
            bl_Lobby lb = FindObjectOfType<bl_Lobby>();
            if (lb != null)
            {
                if (FindObjectOfType<bl_UserProfile>() == null)
                {
                    GameObject inst = PrefabUtility.InstantiatePrefab(prefab, menuScene) as GameObject;
                    GameObject ccb = lb.AddonsButtons[4];
                    if (ccb != null)
                    {
                        inst.transform.SetParent(ccb.transform, false);
                        inst.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        EditorUtility.SetDirty(inst);
                        EditorUtility.SetDirty(lb);
                        EditorSceneManager.MarkSceneDirty(menuScene);
                    }

                    inst = PrefabUtility.InstantiatePrefab(rankingprefab, menuScene) as GameObject;
                    ccb = lb.AddonsButtons[5];
                    inst.transform.SetParent(ccb.transform, false);
                    EditorUtility.SetDirty(inst);

                    Debug.Log("ULogin Pro successfully integrated");
                }
                else
                {
                    Debug.Log("ULogin Pro is already integrated");
                }
            }
            else
            {
                Debug.LogWarning("Can't found the MainMenu scene, that could be cause a change in the default structure of the MFPS folders.");
            }

        }
        else
        {
            Debug.LogWarning("Scenes has not been added in Build Settings, Can't integrate ULogin Pro");
        }

    }
}