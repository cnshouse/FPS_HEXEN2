using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
public class ShaderReplaceEditor : EditorWindow {

	string myString = "Hello World";
	bool groupEnabled;
	bool myBool = true;
	float myFloat = 1.23f;
	//public List<Shader> shaders = new List<Shader>();
	public static Dictionary<string,Shader> shaders= new Dictionary<string,Shader>();
	static List<Material> AllMaterials = new List<Material>();
	List<Material> searchMats = new List<Material>();
	string searchName;
	bool search = false;
	Vector2 scrollPos = Vector2.zero;
	[MenuItem("ReplaceShader/Show Materials")]
	private static void showEditor()
	{
		ShaderReplaceEditor window = (ShaderReplaceEditor)EditorWindow.GetWindow (typeof (ShaderReplaceEditor));
		window.name = "Replace here";
		window.Show();
		findAllMaterial ();
	}
	void OnGUI () {

		float width = this.position.width;
		float height = this.position.height;

		float viewWidth = 1024;
		float viewHeight = 2000;

		scrollPos = GUILayout.BeginScrollView(scrollPos, true, true);
		{
			
			searchName = EditorGUILayout.TextField("search Name", searchName);
			if (GUILayout.Button ("search")) {
				searchName = EditorGUILayout.TextField("search Name", searchName);
				SearchTargetMaterial (searchName);
				search = true;
			}
			int i = 0;
			foreach (Material m in searchMats) {
				EditorGUILayout.ObjectField (((Object)m), typeof(Material), true);
				if (GUILayout.Button ("remove")) {
					searchMats.RemoveAt (i);
					break;
				}
				i++;
			}
			GUILayout.Label("Height", GUILayout.Width(viewWidth), GUILayout.Height(viewHeight));
		}
		GUILayout.EndScrollView();
		GUILayout.Label ("Base Settings", EditorStyles.boldLabel);


	}
	void OnSelectionChange()
	{
		/*Debug.Log (Selection.activeObject.GetInstanceID ());
		if (Selection.activeObject.GetType () == typeof(Shader)) {
			if (!shaders.ContainsKey (Selection.activeObject.GetInstanceID()))
				shaders.Add (((Shader)Selection.activeObject).GetInstanceID(), (Shader)Selection.activeObject);
		}*/
	}

	void SearchTargetMaterial(string s)
	{
		searchMats.Clear ();
		foreach (Material m in AllMaterials) {
			if (m.shader.name == s) {
				searchMats.Add (m);
			}
		}
	}

	static void findAllMaterial()
	{
		AllMaterials = FindAssetsByType<Material> ();
		Debug.Log (AllMaterials.Count);
	}

	static void findAllShader()
	{
		List<Shader> os = FindAssetsByType2<Shader> ();
		foreach (Shader o in os) {
			shaders.Add (o.name, o);
		}
	}
	/*void SaveTargetObjects()
	{
		//load instance id;
		string saveThings = "";
		foreach(KeyValuePair<int,Shader> s in shaders)
		{
			saveThings += s.Key+"|";
		}
		PlayerPrefs.SetString ("targetObjectsID", saveThings);
	}
	void getTargetObjects()
	{
		shaders.Clear();
		string[] sitem = PlayerPrefs.GetString ("targetObjectsID", "").Split ('|');
		for (int i = 0; i < sitem.Count(); i++) {
			if (sitem.Length > 0) {
				Object o = EditorUtility.InstanceIDToObject (int.Parse (sitem [i]));
				if (o != null) {
					shaders.Add (o.GetInstanceID (), (Shader)o);
				}
			}
		}
	}*/

	public static List<T> FindAssetsByType<T>()  where T : UnityEngine.Object
	{
		List<T> assets = new List<T>();
		string[] guids = AssetDatabase.FindAssets("t:Material");
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
