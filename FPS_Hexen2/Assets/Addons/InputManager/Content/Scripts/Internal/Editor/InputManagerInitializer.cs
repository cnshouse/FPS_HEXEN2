using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using MFPSEditor;

public class InputManagerInitializer : MonoBehaviour
{
    private const string DEFINE_KEY = "INPUT_MANAGER";

#if !INPUT_MANAGER
    [MenuItem("MFPS/Addons/InputManager/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if INPUT_MANAGER
    [MenuItem("MFPS/Addons/InputManager/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

   // [MenuItem("MFPS/Addons/InputManager/Lobby Integration")]
    private static void Instegrate()
    {
        GameObject inp = AssetDatabase.LoadAssetAtPath("Assets/Addons/InputManager/Content/Prefabs/UI/Input UI [1.8].prefab", typeof(GameObject)) as GameObject;
        if (inp != null)
        {
            if (SceneManager.sceneCountInBuildSettings > 0)
            {
                if (!EditorSceneManager.GetActiveScene().name.Contains("MainMenu"))
                EditorSceneManager.OpenScene("Assets/MFPS/Scenes/MainMenu.unity", OpenSceneMode.Single);

                GameObject inputWindow = PrefabUtility.InstantiatePrefab(inp, EditorSceneManager.GetActiveScene()) as GameObject;               
                bl_Lobby lb = FindObjectOfType<bl_Lobby>();
                if (lb != null)
                {
                    GameObject ccb = lb.AddonsButtons[1];
                    if (ccb != null)
                    {
                        inputWindow.transform.SetParent(ccb.transform, false);
                        inputWindow.transform.SetAsLastSibling();
                        if (lb.AddonsButtons[8] != null)
                        {
                            lb.AddonsButtons[8].SetActive(false);
                            EditorUtility.SetDirty(lb.AddonsButtons[8]);
                        }
                        EditorUtility.SetDirty(ccb);
                        EditorUtility.SetDirty(inputWindow);                  
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        Debug.Log("<color=green>InputManager integrated in lobby!</color>");
                    }
                }
                else
                {
                    //use U login
                }
            }
            else
            {
                Debug.LogWarning("Scenes has not been added in Build Settings, Can't integrate CC Add-on.");
            }
        }
        else
        {
            Debug.Log("Can't found prefab!");
        }
    }

  /*  [MenuItem("MFPS/Addons/InputManager/Integrate", true)]
    private static bool InstegrateValidate()
    {
        bl_KeyOptionsUI km = GameObject.FindObjectOfType<bl_KeyOptionsUI>();
        return (km == null);
    }*/
}