using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class AssetbundlesMenuItems
{
	
	static List<string> allprefabs = new List<string>();
	static List<string> PrefabWithAssets = new List<string>();
	static List<GameObject> AllPrefabs = new List<GameObject>();
	static List<GameObject> PrefabsWithSystemShader = new List<GameObject>();
	public static Dictionary<string,Shader> SlefShaders= new Dictionary<string,Shader>();

	public static string[] GetAllPrefabs () {
		string[] temp = AssetDatabase.GetAllAssetPaths();
		//List<string> result = new List<string>();
		foreach ( string s in temp ) {
			if ( s.Contains( ".prefab" ) ) allprefabs.Add( s );
		}
		return allprefabs.ToArray();
	}
	[MenuItem ("AssetBundles/Build AssetBundles")]
	static public void BuildAssetBundles ()
	{
        const string kAssetBundlesOutputPath = "../Program_lpj/StreamingAssets/";
		// Choose the output path according to the build target.
		string outputPath = Path.Combine(kAssetBundlesOutputPath, "HeroAnimator"  );
		if (!Directory.Exists(outputPath) )
			Directory.CreateDirectory (outputPath);
        Debug.Log("开始导出~~");
		getAllPrefeb();
		FindObjectsWithSystemMaterial();
		BuildPipeline.BuildAssetBundles (outputPath, 0, EditorUserBuildSettings.activeBuildTarget);
   //     BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        Debug.Log("导出结束~~");
    }


	static void findAllShader()
	{
		List<Shader> os = FindAssetsByType2<Shader> ();
		foreach (Shader o in os) {
				SlefShaders.Add (o.name, o);
		}
	}

	static void getAllPrefeb() {
		AllPrefabs.Clear ();
		allprefabs.Clear ();
		PrefabWithAssets.Clear ();
		GetAllPrefabs ();
		string[] allPrefabs = FindPrefebWithAsssetsBundle();
		foreach ( string prefab in allPrefabs ) {
			UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath( prefab );
			GameObject go;
			try {
				go = (GameObject) o;
				Component[] components = go.GetComponentsInChildren<Component>( true );
				AllPrefabs.Add(go);
			} catch {
				Debug.Log( "For some reason, prefab " + prefab + " won't cast to GameObject" );
			}
		}
	}

	public static string[] FindPrefebWithAsssetsBundle()
	{
		foreach(string s in allprefabs)
		{
			if(AssetImporter.GetAtPath (s).assetBundleName != "")
				PrefabWithAssets.Add (s);
		}
		return PrefabWithAssets.ToArray();
	}

	static void FindObjectsWithSystemMaterial()
	{
		foreach (GameObject go in AllPrefabs) {

			Renderer[] mats = go.GetComponentsInChildren<Renderer> (true);
			bool hasSystemMat = false;
			foreach (Renderer r in mats) {
				if (r.sharedMaterial != null) {
					if (!SlefShaders.ContainsKey (r.sharedMaterial.shader.name)) {
						//这个货用了system shader
						Debug.LogError("GameObject: "+go.name +", "+r.transform.name+" , 用了系统材质");
						hasSystemMat = true;
						continue;
					}
				}
			}
			if(hasSystemMat)
				PrefabsWithSystemShader.Add (go);
		}
	}

	public static List<T> FindAssetsByType2<T>()  where T : UnityEngine.Object
	{
		List<T> assets = new List<T>();
		string[] guids = AssetDatabase.FindAssets("t:Shader");
		for( int i = 0; i < guids.Length; i++ )
		{
			string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
			T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
			if( asset != null )
			{
				assets.Add(asset);
			}
		}
		return assets;
	}
}
