using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class AssetbundleTest
{
	[MenuItem ("AssetBundles/Select AssetBundle")]
	static public void SelectAssetBundle ()
	{
        // Choose the output path according to the build target.
        const string kAssetBundlesOutputPath = "../Program_lpj/StreamingAssets/";
        string outputPath = Path.Combine(kAssetBundlesOutputPath, "HeroAnimator");
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);
        // Build the resource file from the active selection.
        Object obj = Selection.activeObject;
        if (obj == null)
        {
            Debug.Log("需选中模型预设~~");
            return;
        }
        string path = AssetDatabase.GetAssetPath(obj);
        string metapath = AssetDatabase.GetTextMetaFilePathFromAssetPath(path);
        string[] metatxt = File.ReadAllLines(metapath);
        string stemp,bundleName,bundleVariant;
        bundleName = "";
        bundleVariant = "";
        for (int i = 0; i < metatxt.Length; i++)
        {
            stemp = metatxt[i].Replace(" ","");
            if (stemp.Contains("assetBundleName:"))
                bundleName = stemp.Replace("assetBundleName:", "");
            if (stemp.Contains("assetBundleVariant:"))
                bundleVariant = stemp.Replace("assetBundleVariant:", "");
        }
        if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(bundleVariant))
        {
            Debug.Log("预设设置有误~~");
            return;
        }
        Debug.Log("开始导出~~" + bundleName);
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = bundleName;
        buildMap[0].assetBundleVariant = bundleVariant;
        buildMap[0].assetNames = new string[1] { path };
        BuildPipeline.BuildAssetBundles(outputPath, buildMap, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        Debug.Log("导出结束~~");
    }
}
