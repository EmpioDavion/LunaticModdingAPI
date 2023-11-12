using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class MetaConnect : EditorWindow
{
	private UnityEditor.PackageManager.PackageInfo PackageInfo;

	public bool copyDLLs = true;
	public bool copyAssets = true;

	public static string RippedDataPath;
	public static string LunacidPath;
	public static string LunacidDataPath;
	public static string LunaticPath;

	public MetaConnections meta;

	private Vector2 scroll;

	private readonly Dictionary<string, string> GUIDMap = new Dictionary<string, string>();

	[MenuItem("Window/Meta Connect")]
	private static void ShowWindow()
	{
		MetaConnect wnd = GetWindow<MetaConnect>();
		wnd.titleContent = new GUIContent("Meta Connect");

		wnd.Show();
	}

	private void OnEnable()
	{
		PackageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(MetaConnect).Assembly);
		RippedDataPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\GameAssets\\LUNACID\\ExportedProject\\Assets\\";
		LunacidPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\..\\\\LUNACID.exe";
		LunacidDataPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\..\\\\LUNACID_Data\\Managed\\";
		LunaticPath = $"{PackageInfo.resolvedPath}\\";

		if (meta == null)
		{
			string metaPath = $"{PackageInfo.assetPath}\\Editor\\Unity\\Meta Connections.asset";
			meta = AssetDatabase.LoadAssetAtPath<MetaConnections>(metaPath);
		}
	}

	public void OnGUI()
	{
		meta = (MetaConnections)EditorGUILayout.ObjectField("Meta Connections", meta, typeof(MetaConnections), false);

		if (meta == null)
			return;

		scroll = GUILayout.BeginScrollView(scroll);

		for (int i = 0; i < meta.connections.Count; i++)
		{
			MetaConnections.Connection connection = meta.connections[i];

			EditorGUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField("Lunacid Script");
			connection.lunacidScript = EditorGUILayout.TextField(connection.lunacidScript);

			EditorGUILayout.LabelField("Lunatic Script");
			connection.lunaticScript = EditorGUILayout.TextField(connection.lunaticScript);

			if (EditorGUI.EndChangeCheck())
			{
				meta.connections[i] = connection;
				EditorUtility.SetDirty(meta);
			}

			GUI.color = Color.red;

			if (GUILayout.Button("X"))
			{
				Undo.RecordObject(meta, "Deleted script connection");

				meta.connections.RemoveAt(i--);
				EditorUtility.SetDirty(meta);
			}

			GUI.color = Color.white;

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Add New"))
		{
			Undo.RecordObject(meta, "Added script connection");

			meta.connections.Add(new MetaConnections.Connection());
			EditorUtility.SetDirty(meta);
		}

		if (GUILayout.Button("Run"))
			RunAssetCopy();

		copyDLLs = GUILayout.Toggle(copyDLLs, "Copy Lunacid DLLs");
		copyAssets = GUILayout.Toggle(copyAssets, "Copy Lunacid Assets");

		EditorGUILayout.EndHorizontal();
	}

	private void RunAssetCopy()
	{
		try
		{
			if (!Directory.Exists(RippedDataPath))
			{
				string folder = EditorUtility.OpenFolderPanel("AssetRipper Exported Project Folder", LunaticPath, "ExportedProject");

				if (string.IsNullOrEmpty(folder))
					return;

				LunaticPath = folder;
			}

			if (!File.Exists(LunacidPath))
			{
				LunacidPath = EditorUtility.OpenFilePanel("LUNACID.exe File", LunaticPath, "exe");

				if (string.IsNullOrEmpty(LunacidPath))
					return;
			}

			LunacidDataPath = Path.GetDirectoryName(LunacidPath);
			LunacidDataPath = Path.Combine(LunacidDataPath, "\\LUNACID_Data\\Managed\\");

			GUIDMap.Clear();

			if (copyDLLs)
			{
				CopyDLL("Assembly-CSharp");
				CopyDLL("Assembly-CSharp-firstpass");
				CopyDLL("NavMeshComponents");
			}

			CopyScriptMetas();

			if (copyAssets)
			{
				CopyAssetDirectory("AnimationClip");
				CopyAssetDirectory("AudioClip");
				CopyAssetDirectory("Cubemap");
				CopyAssetDirectory("Material");
				CopyAssetDirectory("Mesh");
				CopyAssetDirectory("PhysicMaterial");
				CopyAssetDirectory("PrefabInstance");
				CopyAssetDirectory("RenderTexture");
				CopyAssetDirectory("Resources");
				CopyAssetDirectory("Scenes");
				CopyAssetDirectory("Shader");
				CopyAssetDirectory("Sprite");
				CopyAssetDirectory("Texture2D");
				CopyAssetDirectory("VideoClip");
			}

			ReplaceAssetGUIDs();
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}

		AssetDatabase.Refresh();
	}

	private void CopyScriptMetas()
	{
		string lunacidScriptPath = Path.Combine(RippedDataPath, "Scripts\\Assembly-CSharp\\");
		string lunaticScriptPath = Path.Combine(LunaticPath, "Scripts\\");

		foreach (MetaConnections.Connection connection in meta.connections)
		{
			if (string.IsNullOrEmpty(connection.lunacidScript) || string.IsNullOrEmpty(connection.lunaticScript))
				continue;

			string source = $"{connection.lunacidScript}.cs.meta";
			string dest = $"{connection.lunaticScript}.cs.meta";

			if (!File.Exists(source))
				continue;

			if (!File.Exists(dest))
			{
				File.Copy(source, dest);
				continue;
			}

			string lunacid = ReadMetaGUID(Path.Combine(lunacidScriptPath, source));
			string lunatic = ReadMetaGUID(Path.Combine(lunaticScriptPath, dest));

			GUIDMap.Add(lunacid, lunatic);
		}
	}

	private static void CopyDLL(string dllName)
	{
		string dest = Path.Combine(LunaticPath, "Plugins\\");
		Directory.CreateDirectory(dest);
		dest = Path.Combine(dest, $"{dllName}.dll");

		File.Copy(Path.Combine(LunacidDataPath, $"{dllName}.dll"), dest, true);
	}

	private void CopyAssetDirectory(string dirName)
	{
		string dest = Path.Combine(LunaticPath, "Lunacid\\");
		Directory.CreateDirectory(dest);
		dest = Path.Combine(dest, dirName);
		Directory.CreateDirectory(dest);

		CopyDirectory(Path.Combine(RippedDataPath, dirName), dest);
	}

	private void CopyDirectory(string source, string dest)
	{
		DirectoryInfo sourceDir = new DirectoryInfo(source);
		DirectoryInfo destDir = new DirectoryInfo(dest);

		CopyRecursive(sourceDir, destDir);
	}

	private void CopyRecursive(DirectoryInfo source, DirectoryInfo dest)
	{
		FileInfo[] files = source.GetFiles();
		DirectoryInfo[] subDirs = source.GetDirectories();

		foreach (FileInfo file in files)
		{
			string target = Path.Combine(dest.FullName, file.Name);

			if (file.FullName.EndsWith(".meta") && File.Exists(target))
			{
				string lunacid = ReadMetaGUID(file.FullName);
				string lunatic = ReadMetaGUID(target);

				GUIDMap.Add(lunacid, lunatic);
			}
			else
				file.CopyTo(target, true);
		}

		foreach (DirectoryInfo sourceSubDir in subDirs)
		{
			DirectoryInfo destSubDir = dest.CreateSubdirectory(sourceSubDir.Name);
			CopyRecursive(sourceSubDir, destSubDir);
		}
	}

	private void ReplaceAssetGUIDs()
	{
		string copiedAssetsPath = Path.Combine(LunaticPath, "Lunacid\\");
		DirectoryInfo dir = new DirectoryInfo(copiedAssetsPath);

		ReplaceAssetGUIDsRecursive(dir);
	}

	private void ReplaceAssetGUIDsRecursive(DirectoryInfo dir)
	{
		FileInfo[] files = dir.GetFiles();
		DirectoryInfo[] subDirs = dir.GetDirectories();

		foreach (FileInfo file in files)
		{
			if (file.FullName.EndsWith(".meta"))
				continue;

			string[] lines = File.ReadAllLines(file.FullName);

			bool modified = false;

			for (int i = 0; i < lines.Length; i++)
				modified |= ReplaceAssetGUID(ref lines[i]);

			if (modified)
				File.WriteAllLines(file.FullName, lines);
		}

		foreach (DirectoryInfo subDir in subDirs)
			ReplaceAssetGUIDsRecursive(subDir);
	}

	private bool ReplaceAssetGUID(ref string line)
	{
		const string PATTERN = /* {fileID: ### */", guid: " /* xxx, type: #} */;

		int index = line.IndexOf(PATTERN);

		if (index >= 0)
		{
			int start = index + PATTERN.Length;

			int comma = line.IndexOf(',', start);

			if (comma >= 0)
			{
				string pre = line.Substring(0, start);
				string guid = line.Substring(start, comma - start);
				string post = line.Substring(comma);

				if (GUIDMap.TryGetValue(guid, out string replacement))
					line = string.Concat(pre, replacement, post);

				return true;
			}
		}

		return false;
	}

	private static string ReadMetaGUID(string path)
	{
		string[] lines = File.ReadAllLines(path);

		string guid = System.Array.Find(lines, (x) => x.StartsWith("guid: "));

		return guid.Substring("guid: ".Length);
	}
}
