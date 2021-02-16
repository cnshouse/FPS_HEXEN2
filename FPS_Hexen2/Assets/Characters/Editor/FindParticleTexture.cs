using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
public class FindParticleTexture : EditorWindow
  {
    Vector2 scrollPos = Vector2.zero;
    [MenuItem("ReplaceShader/find Particle Texture")]
    

    private static void showEditor()
    {
        FindParticleTexture window = (FindParticleTexture)EditorWindow.GetWindow(typeof(FindParticleTexture));
        window.name = "Replace here";
        window.Show();
    }
    bool showMateril = false;
    void OnGUI()
    {


        float width = this.position.width;
        float height = this.position.height;

        scrollPos = GUILayout.BeginScrollView(scrollPos, true, true);
        {

            if (GUILayout.Button("particleSystem Texture 大于256"))
            {
                showMateril = true;
                getAllPrefeb();
                FindObjectsWithSystemMaterial();
                Debug.Log(AllPrefabs.Count);
            }

            if (showMateril)
            {
                GUILayout.Label("particle system texture list", EditorStyles.boldLabel);
                foreach (KeyValuePair<string, Texture> s in Sleftextures)
                {
                    EditorGUILayout.ObjectField(((Object)s.Value), typeof(Material), true);
                }
            }

        }
        GUILayout.EndScrollView();

    }
    //查找所有的预设，这里只是拿到预设的名字路径
    static List<string> allprefabs = new List<string>();
    List<string> PrefabWithAssets = new List<string>();
    List<GameObject> AllPrefabs = new List<GameObject>();
    public static Dictionary<string, Texture> Sleftextures = new Dictionary<string, Texture>();
    public static string[] GetAllPrefabs()
    {
        string[] temp = AssetDatabase.GetAllAssetPaths();
        //List<string> result = new List<string>();
        foreach (string s in temp)
        {
            if (s.Contains(".prefab")) allprefabs.Add(s);
        }
        return allprefabs.ToArray();
    }
    void getAllPrefeb()
    {
        AllPrefabs.Clear();
        allprefabs.Clear();
        PrefabWithAssets.Clear();
        GetAllPrefabs();
        string[] allPrefabs = FindPrefebWithAsssetsBundle();
        foreach (string prefab in allPrefabs)
        {
            UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(prefab);
            GameObject go;
            try
            {
                go = (GameObject)o;
                Component[] components = go.GetComponentsInChildren<Component>(true);
                AllPrefabs.Add(go);
            }
            catch
            {
                Debug.Log("For some reason, prefab " + prefab + " won't cast to GameObject");
            }
        }
    }

    public string[] FindPrefebWithAsssetsBundle()
    {
        foreach (string s in allprefabs)
        {
            if (AssetImporter.GetAtPath(s).assetBundleName != "")
                PrefabWithAssets.Add(s);
        }
        return PrefabWithAssets.ToArray();
    }

    void FindObjectsWithSystemMaterial()
    {
        foreach (GameObject go in AllPrefabs)
        {
            ParticleSystem[] ps = go.GetComponentsInChildren<ParticleSystem>(true);
            Renderer[] mats;
            for (int i =0; i < ps.Length; i++)
            {
                mats = ps[i].gameObject.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in mats)
                {
                    if (r.sharedMaterial != null && r.sharedMaterial.mainTexture != null && r.sharedMaterial.mainTexture.width > 256)
                    {
                        if (!Sleftextures.ContainsKey(r.sharedMaterial.mainTexture.name))
                        {
                            //这个货用了system shader
                            Sleftextures.Add(r.sharedMaterial.mainTexture.name, r.sharedMaterial.mainTexture);
                            continue;
                        }
                        else
                        {
                            Debug.Log("same texture name: " + r.sharedMaterial.mainTexture.name);
                        }
                    }
                }
            }
        }
    }
}
