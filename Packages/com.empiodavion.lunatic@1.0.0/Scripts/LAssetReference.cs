using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public struct LAssetReference<T> : ISerializationCallbackReceiver where T : Object
{
	[JsonIgnore]
	public T Reference => reference;

	[SerializeField, JsonIgnore]
	private T reference;

	[SerializeField, HideInInspector, JsonProperty]
	private string assetPath;

	public static implicit operator T(LAssetReference<T> reference)
	{
		return reference.reference;
	}

	public void Load(AssetBundle bundle)
	{
		if (!string.IsNullOrEmpty(assetPath))
			reference = bundle.LoadAsset<T>(assetPath);
	}

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
		if (reference != null)
			assetPath = UnityEditor.AssetDatabase.GetAssetPath(reference);
		else
			assetPath = "";
#endif
	}

	public void OnAfterDeserialize()
	{

	}
}
