using System.IO;
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
				LunacidPath = EditorUtility.OpenFilePanel("LUNACID.exe File", LunacidDataPath, "exe");

				if (string.IsNullOrEmpty(LunacidPath))
					return;
			}

			LunacidDataPath = Path.GetDirectoryName(LunacidPath);
			LunacidDataPath = Path.Combine(LunacidDataPath, "\\LUNACID_Data\\Managed\\");

			if (copyDLLs)
			{
				CopyDLL("Assembly-CSharp");
				CopyDLL("Assembly-CSharp-firstpass");
				CopyDLL("NavMeshComponents");
			}

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

			string lunacidScriptPath = Path.Combine(RippedDataPath, "Scripts\\Assembly-CSharp\\");
			string lunaticScriptPath = Path.Combine(LunaticPath, "Scripts\\");

			foreach (MetaConnections.Connection connection in meta.connections)
			{
				if (string.IsNullOrEmpty(connection.lunacidScript) || string.IsNullOrEmpty(connection.lunaticScript))
					continue;

				string source = $"{connection.lunacidScript}.cs.meta";
				string dest = $"{connection.lunaticScript}.cs.meta";

				if (!File.Exists(source) || !File.Exists(dest))
					continue;

				string[] lines = File.ReadAllLines(Path.Combine(lunacidScriptPath, source));

				string guid = System.Array.Find(lines, (x) => x.StartsWith("guid: "));

				string lunatic = Path.Combine(lunaticScriptPath, dest);
				lines = File.ReadAllLines(lunatic);

				int index = System.Array.FindIndex(lines, (x) => x.StartsWith("guid: "));

				lines[index] = guid;

				File.WriteAllLines(lunatic, lines);
			}
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}

		AssetDatabase.Refresh();
	}

	private void CopyDLL(string dllName)
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
			file.CopyTo(Path.Combine(dest.FullName, file.Name), true);

		foreach (DirectoryInfo sourceSubDir in subDirs)
		{
			DirectoryInfo destSubDir = dest.CreateSubdirectory(sourceSubDir.Name);
			CopyRecursive(sourceSubDir, destSubDir);
		}
	}
}
