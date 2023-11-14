using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public class BuildAssetBundle
{
	[MenuItem("Assets/Build Mod")]
	private static void BuildAllAssetBundles()
	{
		string buildDirectory = "Build";

		if (!Directory.Exists(buildDirectory))
			Directory.CreateDirectory(buildDirectory);

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

			BuildPipeline.BuildAssetBundles(buildDirectory, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
		}

		File.Delete("Build/Build");
		File.Delete("Build/Build.manifest");

		string[] dlls = Directory.GetFiles("Assets/Scripts/", "*.asmdef", SearchOption.AllDirectories);
		string assemblyDir = Path.Combine(Application.dataPath, "../Library/ScriptAssemblies/");

		foreach (string dll in dlls)
		{
			string file = $"{Path.GetFileNameWithoutExtension(dll)}.dll";
			string source = Path.Combine(assemblyDir, file);
			string dest = Path.Combine(buildDirectory, file);
			File.Copy(source, dest, true);
		}
	}

	[MenuItem("Assets/Build Mod and Deploy")]
	private static void BuildAllAssetBundlesAndDeploy()
	{
		BuildAllAssetBundles();

		if (!File.Exists(MetaConnect.LunacidPath))
		{
			MetaConnect.LunacidPath = EditorUtility.OpenFilePanel("LUNACID.exe File", MetaConnect.LunacidDataPath, "exe");

			if (string.IsNullOrEmpty(MetaConnect.LunacidPath))
				return;
		}

		BuildLunaticPlayer();
	}

	private static void BuildLunaticPlayer()
	{
		UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BuildAssetBundle).Assembly);
		string[] scripts = Directory.GetFiles(Path.Combine(packageInfo.resolvedPath, "Scripts\\"), "*.cs");

		AssemblyBuilder builder = new AssemblyBuilder("Lunatic/Lunatic.dll", scripts)
		{
			buildTarget = BuildTarget.StandaloneWindows64,
			buildTargetGroup = BuildTargetGroup.Standalone,
			compilerOptions = new ScriptCompilerOptions()
			{
				ApiCompatibilityLevel = ApiCompatibilityLevel.NET_Standard_2_0,
				CodeOptimization = CodeOptimization.Release,
			},
			referencesOptions = ReferencesOptions.UseEngineModules
		};

		builder.buildFinished += CopyDLLS;

		builder.Build();
	}

	private static void CopyDLLS(string path, CompilerMessage[] messages)
	{
		string gameFolder = Path.GetDirectoryName(MetaConnect.LunacidPath);
		string pluginsFolder = Path.Combine(gameFolder, $"BepInEx/plugins/");
		string deployFolder = Path.Combine(pluginsFolder, PlayerSettings.productName);
		string lunaticFolder = Path.Combine(pluginsFolder, "Lunatic");

		CopyFiles("Build/", deployFolder, "*");
		CopyFiles("Lunatic/", lunaticFolder, "*.dll");
	}

	private static void CopyFiles(string sourceFolder, string destFolder, string searchPattern)
	{
		Directory.CreateDirectory(destFolder);

		string[] files = Directory.GetFiles(sourceFolder, searchPattern);

		foreach (string file in files)
		{
			string filename = Path.GetFileName(file);
			string dest = Path.Combine(destFolder, filename);

			File.Copy(file, dest, true);
		}
	}
}
