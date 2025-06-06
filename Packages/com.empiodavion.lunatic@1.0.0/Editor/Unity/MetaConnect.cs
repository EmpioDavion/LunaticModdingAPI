using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class MetaConnect : EditorWindow
{
	private class MetaGUID
	{
		private struct SubFolder
		{
			public string relativePath;
			public int startIndex;
		}

		public Dictionary<string, string> FileToGUID;
		public Dictionary<string, string> GUIDToFile;

		private readonly SubFolder[] subFolders;

		public MetaGUID(params string[] _subFolders)
		{
			FileToGUID = new Dictionary<string, string>();
			GUIDToFile = new Dictionary<string, string>();

			subFolders = new SubFolder[_subFolders.Length];

			for (int i = 0; i < _subFolders.Length; i++)
				subFolders[i].relativePath = _subFolders[i];
		}

		public string GetSubFolder(int subFolder) => subFolders[subFolder].relativePath;

		public void Add(string file, int subFolder = 0)
		{
			string guid = ReadMetaGUID(file);
			string relative = GetRelativePath(file, subFolder);

			try
			{
				FileToGUID.Add(relative, guid);
				GUIDToFile.Add(guid, relative);
			}
			catch (System.ArgumentException ex)
			{
				Debug.LogError($"Duplicate entry: {relative} - {guid}");
				throw ex;
			}
		}

		public void Clear()
		{
			FileToGUID.Clear();
			GUIDToFile.Clear();
		}

		public void SetIndex(string path)
		{
			for (int i = 0; i < subFolders.Length; i++)
			{
				string fullPath = Path.Combine(path, subFolders[i].relativePath);
				subFolders[i].startIndex = fullPath.Length;
			}
		}

		public string GetRelativePath(string path, int subFolder)
		{
			return path.Substring(subFolders[subFolder].startIndex);
		}
	}

	private class ThreadData
	{
		public string lunaticPackage;
		public ThreadState state;
		public float progress;
		public int progressID;
		public Task task;
	}

	private enum ThreadState
	{
		Ready,
		Running,
		TaskEnded,
		Finished
	}

	private enum RippedDataSubFolders
	{
		Base,
		AssemblyCSharp,
		AssemblyCSharpFirstPass
	}

	private enum LunaticSubFolders
	{
		Base,
		Scripts
	}

	private const bool AllowEditing = false;

	private UnityEditor.PackageManager.PackageInfo PackageInfo;

	public bool copyDLLs = true;
	public bool copyAssets = true;
	public bool copyScripts = true;

	private bool trackMetas = true;

	public static string RippedDataPath;
	public static string LunacidPath;
	public static string LunacidDataPath;
	public static string LunaticPath;

	private static string TextMeshProMeta = "Lunacid\\Plugins\\Unity.TextMeshPro.dll.meta";

	public MetaConnections meta;

	private Vector2 scroll;

	private readonly MetaGUID rippedDataGUIDs = new MetaGUID("", "Scripts\\Assembly-CSharp\\", "Plugins\\Assembly-CSharp-firstpass\\");
	private readonly MetaGUID lunaticGUIDs = new MetaGUID("Lunacid\\", "Scripts\\");

	private string searchLunacid;
	private string searchLunatic;

	private readonly ThreadData threadData = new ThreadData();

	[MenuItem("Window/Meta Connect")]
	private static void ShowWindow()
	{
		MetaConnect wnd = GetWindow<MetaConnect>();
		wnd.titleContent = new GUIContent("Meta Connect");

		wnd.Show();
	}

	private void OnEnable()
	{
		threadData.state = ThreadState.Ready;
		threadData.progress = 0.0f;
		threadData.lunaticPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(MetaConnect).Assembly).resolvedPath;

		PackageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(MetaConnect).Assembly);
		RippedDataPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\GameAssets\\LUNACID\\ExportedProject\\Assets\\";
		LunacidPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\..\\\\LUNACID.exe";
		LunacidDataPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\..\\\\LUNACID_Data\\Managed\\";
		LunaticPath = $"{PackageInfo.resolvedPath}\\";
		TextMeshProMeta = $"{PackageInfo.resolvedPath}\\Lunacid\\Plugins\\Unity.TextMeshPro.dll.meta";

		RippedDataPath = Path.GetFullPath(RippedDataPath);
		LunacidPath = Path.GetFullPath(LunacidPath);
		LunacidDataPath = Path.GetFullPath(LunacidDataPath);

		if (meta == null)
		{
			string metaPath = $"{PackageInfo.assetPath}\\Editor\\Unity\\Meta Connections.asset";
			meta = AssetDatabase.LoadAssetAtPath<MetaConnections>(metaPath);
		}
	}

	public void OnGUI()
	{
		GUI.color = Color.yellow + Color.gray;

		EditorGUILayout.HelpBox(@"Ensure you have opened the 'ExportedProject' from AssetRipper in Unity first!
Unity will generate the .meta files that Meta Connect requires.
Every time LUNACID is updated, you will need to export the game project from AssetRipper and open it in Unity, then run Meta Connect.",
MessageType.Warning);

		GUI.color = Color.white;

		GUI.enabled = AllowEditing;

		meta = (MetaConnections)EditorGUILayout.ObjectField("Meta Connections", meta, typeof(MetaConnections), false);

		if (meta == null)
		{
			GUI.enabled = true;
			return;
		}

		DrawSearchAndSort();
		DrawScripts();
		DrawControls();
		CheckThreadState();
	}

	private void DrawSearchAndSort()
	{
		GUI.enabled = threadData.state != ThreadState.Running;

		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.LabelField("Search", GUILayout.Width(100.0f));
		searchLunacid = EditorGUILayout.TextField(searchLunacid, GUILayout.Width(150.0f));

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("Search", GUILayout.Width(100.0f));
		searchLunatic = EditorGUILayout.TextField(searchLunatic, GUILayout.Width(150.0f));

		EditorGUILayout.LabelField("", GUILayout.Width(40.0f));

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("", GUILayout.Width(32.0f));
		EditorGUILayout.LabelField("", GUILayout.Width(10.0f));

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Separator();

		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.LabelField("", GUILayout.Width(100.0f));

		if (GUILayout.Button("Sort", GUILayout.Width(150.0f)))
		{
			meta.connections.Sort((x, y) => x.lunacidScript.CompareTo(y.lunacidScript));
			EditorUtility.SetDirty(meta);
		}

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("", GUILayout.Width(100.0f));

		if (GUILayout.Button("Sort", GUILayout.Width(150.0f)))
		{
			meta.connections.Sort((x, y) => x.lunaticScript.CompareTo(y.lunaticScript));
			EditorUtility.SetDirty(meta);
		}

		EditorGUILayout.LabelField("", GUILayout.Width(40.0f));

		EditorGUILayout.Separator();

		EditorGUILayout.LabelField("", GUILayout.Width(32.0f));
		EditorGUILayout.LabelField("", GUILayout.Width(10.0f));

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Separator();

	}

	private void DrawScripts()
	{ 
		GUI.enabled = AllowEditing;

		scroll = GUILayout.BeginScrollView(scroll);

		System.Func<string, bool> searchFuncLunacid = SearchNull;
		System.Func<string, bool> searchFuncLunatic = SearchNull;

		if (!string.IsNullOrEmpty(searchLunacid))
			searchFuncLunacid = SearchLunacid;

		if (!string.IsNullOrEmpty(searchLunatic))
			searchFuncLunatic = SearchLunatic;

		for (int i = 0; i < meta.connections.Count; i++)
		{
			MetaConnections.Connection connection = meta.connections[i];

			if (!searchFuncLunacid(connection.lunacidScript) ||
				!searchFuncLunatic(connection.lunaticScript))
				continue;

			EditorGUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField("Lunacid Script", GUILayout.Width(100.0f));
			connection.lunacidScript = EditorGUILayout.TextField(connection.lunacidScript, GUILayout.Width(150.0f));
			
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Lunatic Script", GUILayout.Width(100.0f));
			connection.lunaticScript = EditorGUILayout.TextField(connection.lunaticScript, GUILayout.Width(150.0f));

			connection.isFirstPass = EditorGUILayout.Toggle("First Pass", connection.isFirstPass, GUILayout.Width(40.0f));

			if (EditorGUI.EndChangeCheck())
			{
				meta.connections[i] = connection;
				EditorUtility.SetDirty(meta);
			}

			EditorGUILayout.Separator();

			GUI.color = Color.red;

			if (GUILayout.Button("X", GUILayout.Width(32.0f)))
			{
				Undo.RecordObject(meta, "Deleted script connection");

				meta.connections.RemoveAt(i--);
				EditorUtility.SetDirty(meta);
			}

			GUI.color = Color.white;

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
	}

	private void DrawControls()
	{
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Add New"))
		{
			Undo.RecordObject(meta, "Added script connection");

			meta.connections.Add(new MetaConnections.Connection());
			EditorUtility.SetDirty(meta);
		}

		GUI.enabled = true;

		if (GUILayout.Button("Run"))
			BeginRunAssetCopy();

		copyDLLs = GUILayout.Toggle(copyDLLs, "Copy Lunacid DLLs");
		copyAssets = GUILayout.Toggle(copyAssets, "Copy Lunacid Assets");
		copyScripts = GUILayout.Toggle(copyScripts, "Copy Lunacid Scripts");

		EditorGUILayout.EndHorizontal();

		GUI.enabled = true;

		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(32.0f));

		Rect rect = GUILayoutUtility.GetLastRect();
		EditorGUI.ProgressBar(rect, threadData.progress, threadData.state.ToString());
	}

	private void CheckThreadState()
	{
		if (threadData.state == ThreadState.Running)
			Repaint();
		else if (threadData.state == ThreadState.TaskEnded)
		{
			AssetDatabase.ImportAsset(TextMeshProMeta);

			threadData.state = ThreadState.Finished;

			EditorUtility.SetDirty(meta);
			Repaint();

			AssetDatabase.Refresh();
		}
	}

	private bool SearchNull(string str) => true;

	private bool SearchLunacid(string str) => str.Contains(searchLunacid);
	private bool SearchLunatic(string str) => str.Contains(searchLunatic);

	private void BeginRunAssetCopy()
	{
		if (!Directory.Exists(RippedDataPath))
		{
			string folder = EditorUtility.OpenFolderPanel("AssetRipper 'ExportedProject' Folder", LunaticPath, "ExportedProject");

			if (string.IsNullOrEmpty(folder))
				return;

			RippedDataPath = Path.Combine(folder, "Assets\\");

			string hub = Path.Combine(RippedDataPath, "Scenes\\HUB_01.unity");

			if (!File.Exists(hub))
			{
				Debug.LogError("Could not read project structure, ensure correct folder was selected.");
				return;
			}
		}

		if (!File.Exists(LunacidPath))
		{
			LunacidPath = EditorUtility.OpenFilePanel("LUNACID.exe File", LunaticPath, "exe");

			if (string.IsNullOrEmpty(LunacidPath))
				return;
		}

		threadData.state = ThreadState.Running;
		threadData.progressID = Progress.Start("Running Meta Connect");

		threadData.task = Task.Run(RunAssetCopy);
	}

	private Task RunAssetCopy()
	{
		threadData.state = ThreadState.Running;

		try
		{
			LunacidDataPath = Path.GetDirectoryName(LunacidPath);
			LunacidDataPath = Path.Combine(LunacidDataPath, "LUNACID_Data\\Managed\\");

			meta.Rebuild();

			rippedDataGUIDs.Clear();
			lunaticGUIDs.Clear();

			rippedDataGUIDs.SetIndex(RippedDataPath);
			lunaticGUIDs.SetIndex(LunaticPath);

			if (copyDLLs)
			{
				CopyDLL("Assembly-CSharp");
				CopyDLL("Assembly-CSharp-firstpass");
			}

			AddProgress(0.1f); // 0.1

			if (copyScripts)
				CopyScriptMetas();

			AddProgress(0.1f); // 0.2

			if (copyAssets)
			{
				CopyAssetDirectory("AnimationClip");
				AddProgress(0.04f); // 0.24
				CopyAssetDirectory("AudioClip");
				AddProgress(0.04f); // 0.28
				CopyAssetDirectory("Cubemap");
				AddProgress(0.04f); // 0.32
				CopyAssetDirectory("Material");
				AddProgress(0.04f); // 0.36
				CopyAssetDirectory("Mesh");
				AddProgress(0.04f); // 0.4
				CopyAssetDirectory("PhysicMaterial");
				AddProgress(0.04f); // 0.44
				CopyAssetDirectory("Plugins", false);
				AddProgress(0.04f); // 0.48
				CopyAssetDirectory("PrefabInstance");
				AddProgress(0.04f); // 0.52
				CopyAssetDirectory("RenderTexture");
				AddProgress(0.04f); // 0.56
				CopyAssetDirectory("Resources");

				trackMetas = false;

				AddProgress(0.04f); // 0.6
				CopyAssetDirectory("Scenes");

				trackMetas = true;

				AddProgress(0.04f); // 0.64
				CopyAssetDirectory("Shader");
				AddProgress(0.04f); // 0.69
				CopyAssetDirectory("Sprite");
				AddProgress(0.04f); // 0.72
				CopyAssetDirectory("Texture2D");
				AddProgress(0.04f); // 0.76
				CopyAssetDirectory("VideoClip");
				AddProgress(0.04f); // 0.8
			}

			ReplaceAssetGUIDs();

			ConfigureTextMeshPro();

			SetProgress(1.0f);
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}

		Progress.Remove(threadData.progressID);
		threadData.progress = 0.0f;
		threadData.state = ThreadState.TaskEnded;
		threadData.progressID = -1;

		return null;
	}

	private void SetProgress(float amount)
	{
		threadData.progress = amount;
		Progress.Report(threadData.progressID, threadData.progress);
	}

	private void AddProgress(float amount)
	{
		threadData.progress += amount;
		Progress.Report(threadData.progressID, threadData.progress);
	}

	private void CopyScriptMetas()
	{
		foreach (MetaConnections.Connection connection in meta.connections)
		{
			if (string.IsNullOrEmpty(connection.lunacidScript) || string.IsNullOrEmpty(connection.lunaticScript))
				continue;

			string source = $"{connection.lunacidScript}.cs.meta";
			string dest = $"{connection.lunaticScript}.cs.meta";

			RippedDataSubFolders rippedSub = connection.isFirstPass ? RippedDataSubFolders.AssemblyCSharpFirstPass : RippedDataSubFolders.AssemblyCSharp;
			string rippedSubFolder = rippedDataGUIDs.GetSubFolder((int)rippedSub);

			string lunaticSubFolder = lunaticGUIDs.GetSubFolder((int)LunaticSubFolders.Scripts);

			string sourcePath = Path.Combine(RippedDataPath, rippedSubFolder, source);
			string destPath = Path.Combine(LunaticPath, lunaticSubFolder, dest);

			if (!File.Exists(sourcePath))
			{
				Debug.Log("Failed to find script meta file " + sourcePath);
				continue;
			}

			if (!File.Exists(destPath))
			{
				File.Copy(sourcePath, destPath);
				continue;
			}

			rippedDataGUIDs.Add(sourcePath, (int)(connection.isFirstPass ? RippedDataSubFolders.AssemblyCSharpFirstPass : RippedDataSubFolders.AssemblyCSharp));
			lunaticGUIDs.Add(destPath, (int)LunaticSubFolders.Scripts);
		}
	}

	private static void CopyDLL(string dllName)
	{
		string dest = Path.Combine(LunaticPath, "Plugins\\");
		Directory.CreateDirectory(dest);
		dest = Path.Combine(dest, $"{dllName}.dll");

		File.Copy(Path.Combine(LunacidDataPath, $"{dllName}.dll"), dest, true);
	}

	private void CopyAssetDirectory(string dirName, bool recursive = true)
	{
		string dest = Path.Combine(LunaticPath, "Lunacid\\");
		Directory.CreateDirectory(dest);
		dest = Path.Combine(dest, dirName);
		Directory.CreateDirectory(dest);

		CopyDirectory(Path.Combine(RippedDataPath, dirName), dest, recursive);
	}

	private void CopyDirectory(string source, string dest, bool recursive)
	{
		DirectoryInfo sourceDir = new DirectoryInfo(source);
		DirectoryInfo destDir = new DirectoryInfo(dest);

		CopyRecursive(sourceDir, destDir, recursive);
	}

	private void CopyRecursive(DirectoryInfo source, DirectoryInfo dest, bool recursive)
	{
		FileInfo[] files = source.GetFiles();
		DirectoryInfo[] subDirs = source.GetDirectories();

		foreach (FileInfo file in files)
		{
			string target = Path.Combine(dest.FullName, file.Name);

			if (File.Exists(target))
			{
				if (file.FullName.EndsWith(".meta"))
				{
					if (trackMetas)
					{
						rippedDataGUIDs.Add(file.FullName);
						lunaticGUIDs.Add(target);
					}
				}
				else if (!FilesMatch(file.FullName, target))
					file.CopyTo(target, true);
			}
			else
				file.CopyTo(target, true);
		}

		if (!recursive)
			return;

		foreach (DirectoryInfo sourceSubDir in subDirs)
		{
			DirectoryInfo destSubDir = dest.CreateSubdirectory(sourceSubDir.Name);
			CopyRecursive(sourceSubDir, destSubDir, recursive);
		}
	}

	private bool FilesMatch(string file1, string file2)
	{
		FileInfo fi1 = new FileInfo(file1);
		FileInfo fi2 = new FileInfo(file2);

		byte[] bytes = File.ReadAllBytes(file1);
		byte[] h1 = MD5.Create().ComputeHash(bytes);

		string relativePath = lunaticGUIDs.GetRelativePath(file2, 0);

		if (fi1.Length != fi2.Length)
		{
			meta.SetFileHash(relativePath, h1);
			return false;
		}

		if (!meta.GetFileHash(relativePath, out byte[] h2))
		{
			bytes = File.ReadAllBytes(file2);
			h2 = MD5.Create().ComputeHash(bytes);
		}

		for (int i = 0; i < h1.Length; i++)
		{
			if (h1[i] != h2[i])
			{
				meta.SetFileHash(relativePath, h1);

				return false;
			}
		}

		meta.SetFileHash(relativePath, h2);

		return true;
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

			// approx 6500 files
			AddProgress(0.2f / 6500.0f); // 0.8##
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

				if (RippedDataGUIDToLunaticGUID(guid, out string lunaticGUID))
					line = string.Concat(pre, lunaticGUID, post);

				return true;
			}
		}

		return false;
	}

	private bool RippedDataGUIDToLunaticGUID(string rippedDataGUID, out string lunaticGUID)
	{
		lunaticGUID = null;

		if (rippedDataGUIDs.GUIDToFile.TryGetValue(rippedDataGUID, out string rippedDataFile))
		{
			if (meta.LunacidToLunatic(rippedDataFile, out string lunaticFile))
			{
				if (lunaticGUIDs.FileToGUID.TryGetValue(lunaticFile, out lunaticGUID))
					return true;
				else
					Debug.Log("Failed to find lunatic file " + lunaticFile);
			}
			else if (lunaticGUIDs.FileToGUID.TryGetValue(rippedDataFile, out lunaticGUID))
				return true;
			else
				Debug.Log("Failed to find convert from ripped data file " + rippedDataFile);
		}

		return false;
	}

	private static string ReadMetaGUID(string path)
	{
		string[] lines = File.ReadAllLines(path);

		string guid = System.Array.Find(lines, (x) => x.StartsWith("guid: "));

		return guid.Substring("guid: ".Length);
	}

	private static void ConfigureTextMeshPro()
	{
		string[] lines = File.ReadAllLines(TextMeshProMeta);
		bool getID = false;
		string id = "";

		for (int i = 0; i < lines.Length; i++)
		{
			string trim = lines[i].Trim();

			if (getID)
			{
				id = trim;
				getID = false;
			}
			else if (trim.EndsWith("first:"))
				getID = true;
			else if (trim.StartsWith("Exclude ") ||
				trim.StartsWith("enabled: ") &&
				(id.EndsWith("Win") || id.EndsWith("Win64")))
				lines[i] = lines[i].Substring(0, lines[i].Length - 1) + '1';
		}

		File.WriteAllLines(TextMeshProMeta, lines);
	}
}
