using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class BuildAssetBundle
{
	[MenuItem("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
		string assetBundleDirectory = "Assets/AssetBundles";

		if (!Directory.Exists(assetBundleDirectory))
			Directory.CreateDirectory(assetBundleDirectory);

		string[] bundles = AssetDatabase.GetAllAssetBundleNames();

		List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

		foreach (string bundle in bundles)
		{
			if (bundle == "lunatic")
				continue;

			builds.Add(new AssetBundleBuild()
			{
				assetBundleName = bundle,
				assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundle)
			});

			BuildPipeline.BuildAssetBundles(assetBundleDirectory, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
		}
	}
}
