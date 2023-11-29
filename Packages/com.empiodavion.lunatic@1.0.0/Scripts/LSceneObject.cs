using System;
using UnityEngine;

[Serializable]
public class LSceneObject : ScriptableObject, IModObject, ISerializationCallbackReceiver
{
	public Mod Mod { get; set; }
	public AssetBundle Bundle { get; set; }
	public string Name { get; set; }
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	[Newtonsoft.Json.JsonIgnore]
	public GameObject gameObject;

	public Vector3 localPosition;
	public Vector3 localRotation;
	public Vector3 localScale;
	public string parentTransform;

	[HideInInspector]
	public string gameObjectAssetPath;

	[SerializeField]
	internal LSceneObjectCondition spawnCondition;

	internal void Init()
	{
		gameObject = Bundle.LoadAsset<GameObject>(gameObjectAssetPath);
	}

	public void Spawn(bool ignoreConditions = false)
	{
		if (!ignoreConditions && spawnCondition != null && !spawnCondition.Invoke(this))
			return;

		GameObject parent = GameObject.Find(parentTransform);
		Transform parentTr = parent == null ? null : parent.transform;
		GameObject clone = Instantiate(gameObject, localPosition, Quaternion.Euler(localRotation), parentTr);
		clone.transform.localScale = localScale;

		IModObject[] modObjects = clone.GetComponentsInChildren<IModObject>(true);

		foreach (IModObject modObject in modObjects)
		{
			modObject.Mod = Mod;
			modObject.Bundle = Bundle;
			modObject.AssetName = "";
		}
	}

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
		gameObjectAssetPath = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
#endif
	}

	public void OnAfterDeserialize() { }
}