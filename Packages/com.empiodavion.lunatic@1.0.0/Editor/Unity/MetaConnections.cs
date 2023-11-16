using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lunatic/Meta Connections")]
public class MetaConnections : ScriptableObject
{
	[System.Serializable]
	public struct Connection
	{
		public string lunacidScript;
		public string lunaticScript;
		public bool isFirstPass;
	}

	[System.Serializable]
	public struct FileHash
	{
		public string file;
		public byte[] hash;
	}

	public List<Connection> connections;

	private readonly Dictionary<string, string> lunacidToLunaticDict = new Dictionary<string, string>();

	[SerializeField]
	private List<FileHash> fileHashes;

	private Dictionary<string, byte[]> fileHashDict;

	private void Awake()
	{
		fileHashDict = new Dictionary<string, byte[]>();

		foreach (FileHash hash in fileHashes)
			fileHashDict[hash.file] = hash.hash;
	}

	public void Rebuild()
	{
		lunacidToLunaticDict.Clear();

		foreach (Connection conn in connections)
			lunacidToLunaticDict.Add($"{conn.lunacidScript}.cs.meta", $"{conn.lunaticScript}.cs.meta");
	}

	public bool LunacidToLunatic(string file, out string result)
	{
		return lunacidToLunaticDict.TryGetValue(file, out result);
	}

	public bool GetFileHash(string file, out byte[] hash)
	{
		if (fileHashDict == null)
			Awake();

		return fileHashDict.TryGetValue(file, out hash);
	}

	public void SetFileHash(string file, byte[] hash)
	{
		fileHashDict[file] = hash;
	}
}
