using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public class BuildAssetBundle
{
	const string BUILD_DIR = "Build";
	const string LUNATIC_BUILD_DIR = "LunaticBuild";

	private static void BuildAllAssetBundles()
	{
		string[] bundles = AssetDatabase.GetAllAssetBundleNames();

		if (!Directory.Exists(BUILD_DIR))
			Directory.CreateDirectory(BUILD_DIR);

		if (!Directory.Exists(LUNATIC_BUILD_DIR))
			Directory.CreateDirectory(LUNATIC_BUILD_DIR);

		List<AssetBundleBuild> builds = new List<AssetBundleBuild>
		{
			new AssetBundleBuild()
			{
				assetBundleName = "lunatic",
				assetNames = AssetDatabase.GetAssetPathsFromAssetBundle("lunatic")
			}
		};

		BuildPipeline.BuildAssetBundles(LUNATIC_BUILD_DIR, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

		File.Delete($"{LUNATIC_BUILD_DIR}/{LUNATIC_BUILD_DIR}");
		File.Delete($"{LUNATIC_BUILD_DIR}/{LUNATIC_BUILD_DIR}.manifest");

		builds.Clear();

		foreach (string bundle in bundles)
		{
			if (bundle == "lunatic")
				continue;

			builds.Add(new AssetBundleBuild()
			{
				assetBundleName = bundle,
				assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundle)
			});
		}
		
		BuildPipeline.BuildAssetBundles(BUILD_DIR, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

		File.Delete($"{BUILD_DIR}/{BUILD_DIR}");
		File.Delete($"{BUILD_DIR}/{BUILD_DIR}.manifest");

		CopyAllFilter("Assets/Copy/", BUILD_DIR, (str) => !str.EndsWith(".meta"));

		string[] dlls = Directory.GetFiles("Assets/Scripts/", "*.asmdef", SearchOption.AllDirectories);
		string assemblyDir = Path.Combine(Application.dataPath, "../Library/ScriptAssemblies/");

		foreach (string dll in dlls)
		{
			string file = $"{Path.GetFileNameWithoutExtension(dll)}.dll";
			string source = Path.Combine(assemblyDir, file);
			string dest = Path.Combine(BUILD_DIR, file);
			File.Copy(source, dest, true);
		}

		string newton = typeof(Newtonsoft.Json.JsonConvert).Assembly.Location;
		string newtonName = Path.GetFileName(newton);

		string converters = typeof(Newtonsoft.Json.UnityConverters.Math.Vector3Converter).Assembly.Location;
		string convertersName = Path.GetFileName(converters);

		File.Copy(converters, $"{LUNATIC_BUILD_DIR}/{convertersName}", true);
		File.Copy(newton, $"{LUNATIC_BUILD_DIR}/{newtonName}", true);
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
		Debug.Log("Starting build.");

		UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BuildAssetBundle).Assembly);
		string[] scripts = Directory.GetFiles(Path.Combine(packageInfo.resolvedPath, "Scripts\\"), "*.cs");

		AssemblyBuilder builder = new AssemblyBuilder($"{LUNATIC_BUILD_DIR}/Lunatic.dll", scripts)
		{
			buildTarget = BuildTarget.StandaloneWindows64,
			buildTargetGroup = BuildTargetGroup.Standalone,
			compilerOptions = new ScriptCompilerOptions()
			{
				ApiCompatibilityLevel = ApiCompatibilityLevel.NET_Standard_2_0,
				CodeOptimization = CodeOptimization.Debug
			},
			excludeReferences = new string[] { "Library/ScriptAssemblies/Lunatic.dll" },
			referencesOptions = ReferencesOptions.UseEngineModules
		};

		builder.buildFinished += CopyDLLS;

		builder.Build();
	}

	private static void CopyDLLS(string path, CompilerMessage[] messages)
	{
		foreach (CompilerMessage message in messages)
			if (message.type == CompilerMessageType.Error)
				Debug.LogError(message.message);

		if (messages.Length > 0 && System.Array.Exists(messages, (x) => x.type == CompilerMessageType.Error))
		{
			Debug.LogError("Building mod failed.");
			return;
		}

		string gameFolder = Path.GetDirectoryName(MetaConnect.LunacidPath);
		string pluginsFolder = Path.Combine(gameFolder, $"BepInEx/plugins/");
		string deployFolder = Path.Combine(pluginsFolder, PlayerSettings.productName);
		string lunaticFolder = Path.Combine(pluginsFolder, "Lunatic");

		CopyFiles($"{BUILD_DIR}/", deployFolder, "*");
		CopyFiles($"{LUNATIC_BUILD_DIR}/", lunaticFolder, "*.dll");
		File.Copy($"{LUNATIC_BUILD_DIR}/lunatic", Path.Combine(lunaticFolder, "lunatic"), true);
		File.Copy($"{LUNATIC_BUILD_DIR}/lunatic.manifest", Path.Combine(lunaticFolder, "lunatic.manifest"), true);

		Debug.Log("Completed build..");
	}

	private static void CopyFiles(string sourceFolder, string destFolder, string searchPattern)
	{
		Directory.CreateDirectory(destFolder);

		ShallowCopyDir(sourceFolder, destFolder, searchPattern);

		IEnumerable<string> dirs = Directory.EnumerateDirectories(sourceFolder, "*", SearchOption.AllDirectories);

		foreach (string dir in dirs)
		{
			string destDir = destFolder;

			if (dir.Length > sourceFolder.Length)
			{
				string relative = dir.Substring(sourceFolder.Length);
				destDir = Path.Combine(destFolder, relative);
				Directory.CreateDirectory(destDir);
			}

			ShallowCopyDir(dir, destDir, searchPattern);
		}
	}

	private static void CopyAllFilter(string sourceFolder, string destFolder, 
		System.Predicate<string> filter)
	{
		Directory.CreateDirectory(destFolder);

		ShallowCopyDirFilter(sourceFolder, destFolder, filter);

		IEnumerable<string> dirs = Directory.EnumerateDirectories(sourceFolder, "*", SearchOption.AllDirectories);

		foreach (string dir in dirs)
		{
			string destDir = destFolder;

			if (dir.Length > sourceFolder.Length)
			{
				string relative = dir.Substring(sourceFolder.Length);
				destDir = Path.Combine(destFolder, relative);
				Directory.CreateDirectory(destDir);
			}

			ShallowCopyDirFilter(dir, destDir, filter);
		}
	}

	private static void ShallowCopyDir(string dir, string destFolder, string searchPattern)
	{
		IEnumerable<string> files = Directory.GetFiles(dir, searchPattern);

		foreach (string file in files)
		{
			string filename = Path.GetFileName(file);
			string dest = Path.Combine(destFolder, filename);

			File.Copy(file, dest, true);
		}
	}

	private static void ShallowCopyDirFilter(string dir, string destFolder,
		System.Predicate<string> filter)
	{
		IEnumerable<string> files = Directory.GetFiles(dir);

		foreach (string file in files)
		{
			if (!filter(file))
				continue;

			string filename = Path.GetFileName(file);
			string dest = Path.Combine(destFolder, filename);

			File.Copy(file, dest, true);
		}
	}
}
