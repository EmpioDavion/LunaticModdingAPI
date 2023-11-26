using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public struct LReference<T> : ISerializationCallbackReceiver
{
	[JsonIgnore]
	public T Reference => reference;

	[SerializeField, JsonIgnore]
	private T reference;

	[SerializeField, HideInInspector, JsonProperty]
	private string json;

	public static implicit operator T(LReference<T> reference)
	{
		return reference.reference;
	}

	public void Load()
	{
		if (!string.IsNullOrEmpty(json))
			reference = LJson.Deserialise<T>(json);
	}

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
		if (reference != null)
			json = LJson.Serialise(reference);
		else
			json = "";
#endif
	}

	public void OnAfterDeserialize()
	{
		
	}

	public void Print()
	{
		Debug.Log($"Condition json: " + json);
	}
}
