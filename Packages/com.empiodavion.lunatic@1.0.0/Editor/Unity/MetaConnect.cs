using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class MetaConnect : EditorWindow
{
	private class MetaGUID
	{
		public Dictionary<string, string> FileToGUID;
		public Dictionary<string, string> GUIDToFile;

		private readonly string baseFolder;
		private int startIndex;

		public MetaGUID(string _baseFolder)
		{
			FileToGUID = new Dictionary<string, string>();
			GUIDToFile = new Dictionary<string, string>();
			baseFolder = _baseFolder;
		}

		public void Add(string file)
		{
			string guid = ReadMetaGUID(file);
			string relative = GetRelativePath(file);

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
			startIndex = path.LastIndexOf(baseFolder) + baseFolder.Length;
		}

		public string GetRelativePath(string path)
		{
			return path.Substring(startIndex);
		}

		public bool GetGUID(string fullPath, out string guid)
		{
			string relative = GetRelativePath(fullPath);

			return FileToGUID.TryGetValue(relative, out guid);
		}
	}

	private class ThreadData
	{
		public string lunaticPackage;
		public string tmpPackage;
	}

	private UnityEditor.PackageManager.PackageInfo PackageInfo;

	public bool copyDLLs = true;
	public bool copyAssets = true;
	public bool copyScripts = true;

	private bool trackMetas = true;

	private volatile bool running = false;
	private volatile bool threadFinished = false;

	public static string RippedDataPath;
	public static string LunacidPath;
	public static string LunacidDataPath;
	public static string LunaticPath;

	public MetaConnections meta;

	private Vector2 scroll;

	private readonly MetaGUID rippedDataGUIDs = new MetaGUID("Assets");
	private readonly MetaGUID lunaticGUIDs = new MetaGUID("Lunacid");

	private string rippedDataTMP;
	private string lunaticTMP;
	private string lunaticFont;

	private int textMesh;
	private int fontAsset;

	private volatile float progress = 0.0f;

	private ThreadData activeData;

	[MenuItem("Window/Meta Connect")]
	private static void ShowWindow()
	{
		MetaConnect wnd = GetWindow<MetaConnect>();
		wnd.titleContent = new GUIContent("Meta Connect");

		wnd.Show();
	}

	private void OnEnable()
	{
		running = false;
		threadFinished = false;
		progress = 0.0f;

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
	}

	public void OnGUI()
	{
		GUI.enabled = !running;

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
		{
			running = true;

			System.Threading.ThreadPool.QueueUserWorkItem(RunAssetCopy, new ThreadData()
			{
				tmpPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMPro.TextMeshProUGUI).Assembly).resolvedPath,
				lunaticPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(MetaConnect).Assembly).resolvedPath
			});
			//RunAssetCopy();
		}

		copyDLLs = GUILayout.Toggle(copyDLLs, "Copy Lunacid DLLs");
		copyAssets = GUILayout.Toggle(copyAssets, "Copy Lunacid Assets");
		copyScripts = GUILayout.Toggle(copyScripts, "Copy Lunacid Scripts");

		EditorGUILayout.EndHorizontal();

		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(32.0f));

		Rect rect = GUILayoutUtility.GetLastRect();
		EditorGUI.ProgressBar(rect, progress, running ? "Running Meta Connect" : "Ready");

		GUI.enabled = true;

		if (running)
			Repaint();
		else if (threadFinished)
		{
			threadFinished = false;
			EditorUtility.SetDirty(meta);
			Repaint();
		}
	}

	private void RunAssetCopy(object state)
	{
		running = true;

		activeData = (ThreadData)state;

		try
		{
			if (!Directory.Exists(RippedDataPath))
			{
				string folder = EditorUtility.OpenFolderPanel("AssetRipper Exported Project Folder", LunaticPath, "ExportedProject");

				if (string.IsNullOrEmpty(folder))
					return;

				RippedDataPath = folder;
			}

			if (!File.Exists(LunacidPath))
			{
				LunacidPath = EditorUtility.OpenFilePanel("LUNACID.exe File", LunaticPath, "exe");

				if (string.IsNullOrEmpty(LunacidPath))
					return;
			}

			LunacidDataPath = Path.GetDirectoryName(LunacidPath);
			LunacidDataPath = Path.Combine(LunacidDataPath, "LUNACID_Data\\Managed\\");

			rippedDataGUIDs.Clear();
			lunaticGUIDs.Clear();

			rippedDataGUIDs.SetIndex(RippedDataPath);
			lunaticGUIDs.SetIndex(Path.Combine(LunaticPath, "Lunacid\\"));

			if (copyDLLs)
			{
				CopyDLL("Assembly-CSharp");
				CopyDLL("Assembly-CSharp-firstpass");
				CopyDLL("NavMeshComponents");
			}

			progress = 0.1f; // 0.1

			if (copyScripts)
				CopyScriptMetas();

			progress += 0.1f; // 0.2

			if (copyAssets)
			{
				CopyAssetDirectory("AnimationClip");
				progress += 0.05f; // 0.25
				CopyAssetDirectory("AudioClip");
				progress += 0.05f; // 0.3
				CopyAssetDirectory("Cubemap");
				progress += 0.05f; // 0.35
				CopyAssetDirectory("Material");
				progress += 0.05f; // 0.4
				CopyAssetDirectory("Mesh");
				progress += 0.05f; // 0.45
				CopyAssetDirectory("PhysicMaterial");
				progress += 0.05f; // 0.5
				CopyAssetDirectory("PrefabInstance");
				progress += 0.05f; // 0.55
				CopyAssetDirectory("RenderTexture");
				progress += 0.05f; // 0.6
				CopyAssetDirectory("Resources");

				trackMetas = false;

				progress += 0.05f; // 0.65
				CopyAssetDirectory("Scenes");

				trackMetas = true;

				progress += 0.05f; // 0.7
				CopyAssetDirectory("Shader");
				progress += 0.05f; // 0.75
				CopyAssetDirectory("Sprite");
				progress += 0.05f; // 0.8
				CopyAssetDirectory("Texture2D");
				progress += 0.05f; // 0.85
				CopyAssetDirectory("VideoClip");
				progress += 0.05f; // 0.9
			}

			ReplaceAssetGUIDs();

			progress += 0.1f; // 1.0
		}
		catch (System.Exception e)
		{
			Debug.Log(e);
		}

		AssetDatabase.Refresh();

		progress = 0.0f;
		threadFinished = true;
		running = false;
	}

	private void CopyScriptMetas()
	{
		string rippedDataScriptPath = Path.Combine(RippedDataPath, "Scripts\\Assembly-CSharp\\");
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

			rippedDataGUIDs.Add(Path.Combine(rippedDataScriptPath, source));
			lunaticGUIDs.Add(Path.Combine(lunaticScriptPath, source));
		}

		//UnityEditor.PackageManager.PackageInfo lunaticTMPPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMPro.TextMeshProUGUI).Assembly);

		string rippedTMPPath = Path.Combine(RippedDataPath, "Plugins\\Unity.TextMeshPro.dll.meta");
		string lunaticTMPPath = Path.Combine(activeData.tmpPackage, "Scripts\\Runtime\\TextMeshProUGUI.cs.meta");
		string lunaticFontPath = Path.Combine(activeData.tmpPackage, "Scripts\\Runtime\\TMP_FontAsset.cs.meta");

		rippedDataTMP = ReadMetaGUID(rippedTMPPath);
		lunaticTMP = ReadMetaGUID(lunaticTMPPath);
		lunaticFont = ReadMetaGUID(lunaticFontPath);
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

		string relativePath = lunaticGUIDs.GetRelativePath(file2);

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

				if (rippedDataGUIDs.GUIDToFile.TryGetValue(guid, out string file) &&
					lunaticGUIDs.FileToGUID.TryGetValue(file, out guid))
					line = string.Concat(pre, guid, post);
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

					line = string.Concat(preFileID, "11500000", PATTERN, guid, post);
				}

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
