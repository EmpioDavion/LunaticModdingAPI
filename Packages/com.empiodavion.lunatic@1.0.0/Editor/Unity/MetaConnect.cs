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

			FileToGUID.Add(relative, guid);
			GUIDToFile.Add(guid, relative);
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
		public string tmpPackage;
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

	private UnityEditor.PackageManager.PackageInfo PackageInfo;

	public bool copyDLLs = true;
	public bool copyAssets = true;
	public bool copyScripts = true;

	private bool trackMetas = true;

	public static string RippedDataPath;
	public static string LunacidPath;
	public static string LunacidDataPath;
	public static string LunaticPath;

	public MetaConnections meta;

	private Vector2 scroll;

	private readonly MetaGUID rippedDataGUIDs = new MetaGUID("", "Scripts\\Assembly-CSharp\\", "Plugins\\Assembly-CSharp-firstpass\\");
	private readonly MetaGUID lunaticGUIDs = new MetaGUID("Lunacid\\", "Scripts\\");

	#region TextMeshPro

	private string rippedDataTMP;
	private string lunaticTMP;
	private string lunaticFont;
	private string lunaticSprite;
	private string lunaticStyle;

	private int textMesh;
	private int fontAsset;
	private int spriteAsset;
	private int styleSheet;

	#endregion

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
		threadData.tmpPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMPro.TextMeshProUGUI).Assembly).resolvedPath;
		threadData.lunaticPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(MetaConnect).Assembly).resolvedPath;

		PackageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(MetaConnect).Assembly);
		RippedDataPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\GameAssets\\LUNACID\\ExportedProject\\Assets\\";
		LunacidPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\..\\\\LUNACID.exe";
		LunacidDataPath = $"{PackageInfo.resolvedPath}\\..\\..\\..\\..\\\\LUNACID_Data\\Managed\\";
		LunaticPath = $"{PackageInfo.resolvedPath}\\";

		RippedDataPath = Path.GetFullPath(RippedDataPath);
		LunacidPath = Path.GetFullPath(LunacidPath);
		LunacidDataPath = Path.GetFullPath(LunacidDataPath);

		if (meta == null)
		{
			string metaPath = $"{PackageInfo.assetPath}\\Editor\\Unity\\Meta Connections.asset";
			meta = AssetDatabase.LoadAssetAtPath<MetaConnections>(metaPath);
		}

		textMesh = FileIDUtil.Compute<TMPro.TextMeshProUGUI>();
		fontAsset = FileIDUtil.Compute<TMPro.TMP_FontAsset>();
		spriteAsset = FileIDUtil.Compute<TMPro.TMP_SpriteAsset>();
		styleSheet = FileIDUtil.Compute<TMPro.TMP_StyleSheet>();
	}

	public void OnGUI()
	{
		GUI.enabled = threadData.state != ThreadState.Running;

		meta = (MetaConnections)EditorGUILayout.ObjectField("Meta Connections", meta, typeof(MetaConnections), false);

		if (meta == null)
			return;

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

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Add New"))
		{
			Undo.RecordObject(meta, "Added script connection");

			meta.connections.Add(new MetaConnections.Connection());
			EditorUtility.SetDirty(meta);
		}

		if (GUILayout.Button("Run"))
			BeginRunAssetCopy();

		copyDLLs = GUILayout.Toggle(copyDLLs, "Copy Lunacid DLLs");
		copyAssets = GUILayout.Toggle(copyAssets, "Copy Lunacid Assets");
		copyScripts = GUILayout.Toggle(copyScripts, "Copy Lunacid Scripts");

		EditorGUILayout.EndHorizontal();

		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(32.0f));

		Rect rect = GUILayoutUtility.GetLastRect();
		EditorGUI.ProgressBar(rect, threadData.progress, threadData.state.ToString());

		GUI.enabled = true;

		if (threadData.state == ThreadState.Running)
			Repaint();
		else if (threadData.state == ThreadState.TaskEnded)
		{
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
			string sln = EditorUtility.OpenFilePanel("AssetRipper Exported Project Solution", LunaticPath, "sln");

			if (string.IsNullOrEmpty(sln))
				return;

			RippedDataPath = Path.Combine(Path.GetDirectoryName(sln), "Assets\\");
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
				CopyDLL("NavMeshComponents");
			}

			AddProgress(0.1f); // 0.1

			if (copyScripts)
				CopyScriptMetas();

			AddProgress(0.1f); // 0.2

			if (copyAssets)
			{
				CopyAssetDirectory("AnimationClip");
				AddProgress(0.05f); // 0.25
				CopyAssetDirectory("AudioClip");
				AddProgress(0.05f); // 0.3
				CopyAssetDirectory("Cubemap");
				AddProgress(0.05f); // 0.35
				CopyAssetDirectory("Material");
				AddProgress(0.05f); // 0.4
				CopyAssetDirectory("Mesh");
				AddProgress(0.05f); // 0.45
				CopyAssetDirectory("PhysicMaterial");
				AddProgress(0.05f); // 0.5
				CopyAssetDirectory("PrefabInstance");
				AddProgress(0.05f); // 0.55
				CopyAssetDirectory("RenderTexture");
				AddProgress(0.05f); // 0.6
				CopyAssetDirectory("Resources");

				trackMetas = false;

				AddProgress(0.05f); // 0.65
				CopyAssetDirectory("Scenes");

				trackMetas = true;

				AddProgress(0.05f); // 0.7
				CopyAssetDirectory("Shader");
				AddProgress(0.05f); // 0.75
				CopyAssetDirectory("Sprite");
				AddProgress(0.05f); // 0.8
				CopyAssetDirectory("Texture2D");
				AddProgress(0.05f); // 0.85
				CopyAssetDirectory("VideoClip");
				AddProgress(0.05f); // 0.9
			}

			ReplaceAssetGUIDs();

			AddProgress(0.1f); // 1.0
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

		string rippedTMPPath = Path.Combine(RippedDataPath, "Plugins\\Unity.TextMeshPro.dll.meta");
		string lunaticTMPPath = Path.Combine(threadData.tmpPackage, "Scripts\\Runtime\\TextMeshProUGUI.cs.meta");
		string lunaticFontPath = Path.Combine(threadData.tmpPackage, "Scripts\\Runtime\\TMP_FontAsset.cs.meta");
		string lunaticSpritePath = Path.Combine(threadData.tmpPackage, "Scripts\\Runtime\\TMP_SpriteAsset.cs.meta");
		string lunaticStylePath = Path.Combine(threadData.tmpPackage, "Scripts\\Runtime\\TMP_StyleSheet.cs.meta");
		
		rippedDataTMP = ReadMetaGUID(rippedTMPPath);
		lunaticTMP = ReadMetaGUID(lunaticTMPPath);
		lunaticFont = ReadMetaGUID(lunaticFontPath);
		lunaticSprite = ReadMetaGUID(lunaticSpritePath);
		lunaticStyle = ReadMetaGUID(lunaticStylePath);
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

		foreach (DirectoryInfo sourceSubDir in subDirs)
		{
			DirectoryInfo destSubDir = dest.CreateSubdirectory(sourceSubDir.Name);
			CopyRecursive(sourceSubDir, destSubDir);
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
				else if (guid == rippedDataTMP)
				{
					const string ID_PATTERN = "fileID: ";

					index = line.IndexOf(ID_PATTERN);
					start = index + ID_PATTERN.Length;
					comma = line.IndexOf(',', start);
					string preFileID = pre.Substring(0, start);
					string fileIDStr = pre.Substring(start, comma - start);
					int fileID = int.Parse(fileIDStr);

					if (fileID == textMesh)
						guid = lunaticTMP;
					else if (fileID == fontAsset)
						guid = lunaticFont;
					else if (fileID == spriteAsset)
						guid = lunaticSprite;
					else if (fileID == styleSheet)
						guid = lunaticStyle;

					line = string.Concat(preFileID, "11500000", PATTERN, guid, post);
				}

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
}
