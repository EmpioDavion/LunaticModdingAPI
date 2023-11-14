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
	}

	[System.Serializable]
	public struct FileHash
	{
		public string file;
		public byte[] hash;
	}

	public List<Connection> connections;

	[SerializeField]
	private List<FileHash> fileHashes;

	private Dictionary<string, byte[]> fileHashDict;

	private void Awake()
	{
		fileHashDict = new Dictionary<string, byte[]>();

		foreach (FileHash hash in fileHashes)
			fileHashDict[hash.file] = hash.hash;
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
