using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
public class ReplaceShader : EditorWindow {

	bool groupEnabled;
	bool NoShader = false;
	//public List<Shader> shaders = new List<Shader>();
	public static Dictionary<string,Shader> shaders= new Dictionary<string,Shader>();  //所有可以在assets里找到的shader,如果用户输入后发现shader没有在这个列表中，还会在后面增加到这个列表中
	static List<Material> AllMaterials = new List<Material>(); //所有的材质球
	static List<Material> MaterialWithSystemShader = new List<Material>();  //使用了系统shader的材质球
	public static Dictionary<string,Shader> SystemShaders= new Dictionary<string,Shader>();  //系统shader列表
	public static Dictionary<string,Shader> CurrentUsedShaderWithGL= new Dictionary<string,Shader>();  //现在使用中的gl开头的shader列表
	Dictionary<string,Shader> addedShader = new Dictionary<string,Shader>();  //已增加的shader，在窗口中输入shader名字后点击add会增加到这个列表
	public static Dictionary<string,Shader> SlefShaders= new Dictionary<string,Shader>();  //这个是自己现在使用的shader，使用它是为了和所有的shader区分来找出系统shader
	//List<Shader> searchMats = new List<Material>();
	string searchName;
	Vector2 scrollPos = Vector2.zero;
	List<GameObject> AllPrefabs = new List<GameObject>();
	List<GameObject> PrefabsWithSystemShader = new List<GameObject>();
	bool showPrefab = false;
	bool showMaterial = false;
	bool showShader = false;
	bool showShaderWithGL = false;
	[MenuItem("ReplaceShader/Show Shader")]
	private static void showEditor()
	{
		ReplaceShader window = (ReplaceShader)EditorWindow.GetWindow (typeof (ReplaceShader));
		window.name = "Replace here";
		window.Show();
		findAllShader ();
		findAllMaterial ();
	}
	void OnGUI () {


		float width = this.position.width;
		float height = this.position.height;

		float viewWidth = 1024;
		float viewHeight = 2000;

		scrollPos = GUILayout.BeginScrollView(scrollPos, true, true);
		{
			searchName = EditorGUILayout.TextField ("search Name", searchName);
			if (NoShader) {
				GUILayout.Label ("没有这个Shader，请重新输入", EditorStyles.boldLabel);
			} else {
				GUILayout.Label ("有这个Shader", EditorStyles.boldLabel);
			}
			if (GUILayout.Button ("Add")) {
				searchName = EditorGUILayout.TextField ("search Name", searchName);
				SaveTargetObjects (searchName);
			}
			if (GUILayout.Button ("Remove")) {
				searchName = EditorGUILayout.TextField ("search Name", searchName);
				RemoveTargetObjects (searchName);
			}
			if (GUILayout.Button ("SHOW HISTORY")) {
				getTargetObjects ();
			}
			if (GUILayout.Button ("Replace")) {
				Replace ();
			}
			if (GUILayout.Button ("系统的材质球")) {
				showPrefab = true;
				showMaterial = false;
				showShader = false;
				showShaderWithGL = false;
				getAllPrefeb ();
				FindObjectsWithSystemMaterial ();
				Debug.Log (AllPrefabs.Count);
			}
			if (GUILayout.Button ("系统的shader")) {
				showPrefab = false;
				showMaterial = true;
				showShader = false;
				showShaderWithGL = false;
				FindMaterialWithSystemShader ();
				Debug.Log (MaterialWithSystemShader.Count);
			}
			if (GUILayout.Button ("系统的shader列表")) {
				showPrefab = false;
				showMaterial = false;
				showShader = true;
				showShaderWithGL = false;
				FindMaterialWithSystemShader ();
			}
			if (GUILayout.Button ("现在使用中的带有GL的shader")) {
				showPrefab = false;
				showMaterial = false;
				showShader = false;
				showShaderWithGL = true;
				FindUsedShaderWithGL ();
			}
			foreach (KeyValuePair<string,Shader> s in addedShader) {
				EditorGUILayout.ObjectField (((Object)s.Value), typeof(Shader), true);
			}
			if(showPrefab)
			{
			  GUILayout.Label ("Prefab With System Shader", EditorStyles.boldLabel);
				foreach (GameObject go in PrefabsWithSystemShader) {
					EditorGUILayout.ObjectField (((Object)go), typeof(GameObject), true);
			    }
			}
			if (showMaterial) {
				GUILayout.Label ("Material With System shader", EditorStyles.boldLabel);
				foreach (Material go in MaterialWithSystemShader) {
					EditorGUILayout.ObjectField (((Object)go), typeof(Material), true);
				}

			}
			if (showShader) {
				GUILayout.Label ("System shader list", EditorStyles.boldLabel);
				foreach (KeyValuePair<string,Shader> s in SystemShaders) {
					EditorGUILayout.ObjectField (((Object)s.Value), typeof(Shader), true);
				}
			}
			if (showShaderWithGL) {
				GUILayout.Label ("System shader list", EditorStyles.boldLabel);
				foreach (KeyValuePair<string,Shader> s in CurrentUsedShaderWithGL) {
					EditorGUILayout.ObjectField (((Object)s.Value), typeof(Shader), true);
				}
			}
		}
		GUILayout.EndScrollView();

	}
	void OnSelectionChange()
	{
		/*Debug.Log (Selection.activeObject.GetInstanceID ());
		if (Selection.activeObject.GetType () == typeof(Shader)) {
			if (!shaders.ContainsKey (Selection.activeObject.GetInstanceID()))
				shaders.Add (((Shader)Selection.activeObject).GetInstanceID(), (Shader)Selection.activeObject);
		}*/
	}
	//查找所有的材质球
	static void findAllMaterial()
	{
		AllMaterials = FindAssetsByType<Material> ();
		Debug.Log (AllMaterials.Count);
	}

	//找到所有shader在assets目录中
	static void findAllShader()
	{
		List<Shader> os = FindAssetsByType2<Shader> ();
		foreach (Shader o in os) {
			if (!shaders.ContainsKey (o.name)) {
				shaders.Add (o.name, o);
				/*if (o.name== "Mobile/Particles/Alpha Blended") {
					Debug.Log ("Mobile/Particles/Alpha Blended");
				}*/
				SlefShaders.Add (o.name, o);
			}
		}
		Debug.Log (shaders.Count);
	}

	//将用户输入的shader名字查找并保存,如果用户输入的shader不存在在shaders中，那么他加入到shaders中，那么这个shader将作为系统shader对待
	void SaveTargetObjects(string s)
	{
		//load instance id;
		if (!shaders.ContainsKey (s)) {
			Shader systemShader = Shader.Find (s);
			if (!systemShader) {
				NoShader = true;
				return;
			} else {
				shaders.Add (s, systemShader);
			}
		}
		if (!addedShader.ContainsKey (s)) {
			NoShader = false;
			addedShader.Add(s, shaders[s]);
			string saveThings = LoadFile(Application.dataPath, "ReplaceShader.txt");
			saveThings += s + "|";

			CreateOrOPenFile(Application.dataPath, "ReplaceShader.txt", saveThings);
			//PlayerPrefs.SetString ("targetObjectsID", saveThings);
		}
	}
	//删除以前保存过的shader
	void RemoveTargetObjects(string s)
	{
		if (addedShader.ContainsKey (s)) {
			addedShader.Remove(s);
			string saveThings = "";
			PlayerPrefs.SetString ("targetObjectsID", "");
			foreach (KeyValuePair<string,Shader> sh in addedShader) {
				saveThings += sh.Key+ "|";
			}
			//PlayerPrefs.SetString ("targetObjectsID", saveThings);
			CreateOrOPenFile(Application.dataPath, "ReplaceShader.txt", saveThings);
		}
	}
	//这是将保存过得shader显示出来
	void getTargetObjects()
	{
		//string[] sitem = PlayerPrefs.GetString ("targetObjectsID", "").Split ('|');
		if (LoadFile (Application.dataPath, "ReplaceShader.txt") != null) {
			string[] sitem = LoadFile (Application.dataPath, "ReplaceShader.txt").Split ('|');
			for (int i = 0; i < sitem.Count (); i++) {
				if (sitem.Length > 0) {
					if (shaders.ContainsKey (sitem [i]) && !addedShader.ContainsKey (sitem [i])) {
						addedShader.Add (sitem [i], shaders [sitem [i]]);
					}
					if (!shaders.ContainsKey (sitem [i])) {
						Shader systemShader = Shader.Find (sitem [i]);
						if (!systemShader) {
							continue;
						} else {
							shaders.Add (sitem [i], systemShader);
							addedShader.Add (sitem [i], systemShader);
						}
					}
				}
			}
		}
	}
	//这个是替换，执行后，会将所有现在增加过的shader不带GL的shader转换为带GL的shader
	void Replace()
	{
		foreach (Material m in AllMaterials) {
			foreach (KeyValuePair<string,Shader> s in addedShader) {
				if (m.shader.name == s.Key) {
					if (shaders.ContainsKey ("GL/" + s.Key)) {
						m.shader = shaders ["GL/" + s.Key];
					} else {
						Debug.LogError ("没有这个shader: " + "GL/" + s.Key);
					}
				}
			}
		}
	}
	//在asstes中查找材质球
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

	//在asstes中查找shader
	public static List<T> FindAssetsByType2<T>()  where T : UnityEngine.Object
	{
		List<T> assets = new List<T>();
		string[] guids = AssetDatabase.FindAssets("t:Shader");
		for( int i = 0; i < guids.Length; i++ )
		{
			string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );

			T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
			//Debug.Log (asset.name);
			//因为在shade/sd/文件下，这个shader是存在的，所以我的判断认为他不是系统材质球，因为这个shader和系统的shader命名一样，所以将他排除在外。
			if (!asset.name.Contains ("Mobile/Particles")) {
				Debug.Log (assetPath);
				if (asset != null) {
					assets.Add (asset);
				}
			} else if (asset.name.Contains ("GL")) {
				if (asset != null) {
					assets.Add (asset);
				}
			}
		}
		return assets;
	}
	//查找所有的预设，这里只是拿到预设的名字路径
	static List<string> allprefabs = new List<string>();
	List<string> PrefabWithAssets = new List<string>();
	public static string[] GetAllPrefabs () {
		string[] temp = AssetDatabase.GetAllAssetPaths();
		//List<string> result = new List<string>();
		foreach ( string s in temp ) {
			if ( s.Contains( ".prefab" ) ) allprefabs.Add( s );
		}
		 return allprefabs.ToArray();
	}

	//真正通过上面的方法拿到预设
	void getAllPrefeb() {
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
	//查找所有可以被打包的预设的路径
	public string[] FindPrefebWithAsssetsBundle()
	{
		foreach(string s in allprefabs)
		{
			if(AssetImporter.GetAtPath (s).assetBundleName != "")
			PrefabWithAssets.Add (s);
		}
		return PrefabWithAssets.ToArray();
	}

	//拿到真正的预设，这个预设是可以被打包的预设
	void FindObjectsWithSystemMaterial()
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

	//查找材质球使用了系统shader
	void FindMaterialWithSystemShader()
	{
		foreach (Material m in AllMaterials) {
			if (!m.shader.name.Contains ("GL")) {
				if (!SlefShaders.ContainsKey (m.shader.name)) {
					//这个货用了system shader
					MaterialWithSystemShader.Add (m);
					if (!SystemShaders.ContainsKey (m.shader.name)) {
						SystemShaders.Add (m.shader.name, m.shader);
					}
					//Debug.LogError("Material: "+m.name +" , 用了系统材质");
					continue;
				}
			}
			}
	}
	//现在材质球中使用的带有GL的shader
	void FindUsedShaderWithGL()
	{
		foreach (Material m in AllMaterials) {
			if (m.shader != null) {
				if (m.shader.name.Contains ("GL")) {
					if (!CurrentUsedShaderWithGL.ContainsKey (m.shader.name)) {
						//这个货用了system shader
						CurrentUsedShaderWithGL.Add(m.shader.name,m.shader);
						//Debug.LogError("Material: "+m.name +" , 用了系统材质");
						continue;
					}
				}
			}
		}
	}
	void CreateOrOPenFile(string path, string name, string info)
	{

		StreamWriter sw;
		FileInfo fi = new FileInfo(path + "//" + name);
		if (!fi.Exists)
		{
			sw = fi.CreateText();
		}
		else
		{
			fi.Delete ();
			sw = fi.AppendText();
		}
		sw.Write (info);
		//sw.WriteLine(info);
		sw.Close();
	}

	string LoadFile(string path, string name)
	{
		StreamReader sr = null;
		try
		{
			sr = File.OpenText(path + "//" + name);
		}
		catch
		{
			return null;
		}
		string lineInfo = "";

		lineInfo += sr.ReadLine();
		sr.Close();
		return lineInfo;
	}

}
