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

	public System.Collections.Generic.List<Connection> connections;
}
