using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MFPSEditor;

public class ShopAddonInitialiazer : MonoBehaviour
{
    private const string DEFINE_KEY = "SHOP";

#if !SHOP
    [MenuItem("MFPS/Addons/Shop/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if SHOP
    [MenuItem("MFPS/Addons/Shop/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/Shop/Integrate")]
    private static void Instegrate()
    {
        if (AssetDatabase.IsValidFolder("Assets/MFPS/Scenes"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            GameObject inp = AssetDatabase.LoadAssetAtPath("Assets/Addons/Shop/Prefabs/ShopUI.prefab", typeof(GameObject)) as GameObject;
            if (inp != null)
            {
                if (EditorSceneManager.sceneCountInBuildSettings > 0)
                {
                    EditorSceneManager.OpenScene("Assets/MFPS/Scenes/MainMenu.unity", OpenSceneMode.Single);
                    bl_Lobby lb = FindObjectOfType<bl_Lobby>();
                    if (lb != null)
                    {
                        GameObject pr = PrefabUtility.InstantiatePrefab(inp, EditorSceneManager.GetActiveScene()) as GameObject;
                        GameObject ccb = bl_LobbyUI.Instance.AddonsButtons[12];
                        if (ccb != null) { pr.transform.SetParent(ccb.transform, false); }
                        EditorUtility.SetDirty(pr);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        Debug.Log("<color=#green>Shop addon integrate!</color>");
                    }
                }
                else
                {
                    Debug.LogWarning("Scenes has not been added in Build Settings, Can't integrate Shop Add-on.");
                }
            }
            else
            {
                Debug.Log("Can't found prefab!");
            }
        }
        else
        {
            Debug.LogWarning("Can't integrate the addons because MFPS folder structure has been change, please do the manual integration.");
        }
    }

    [MenuItem("MFPS/Addons/Shop/Integrate", true)]
    private static bool IntegrateValidate()
    {
        bl_Lobby km = GameObject.FindObjectOfType<bl_Lobby>();
        bl_ShopManager gm = GameObject.FindObjectOfType<bl_ShopManager>();
        return (km != null && gm == null);
    }
}