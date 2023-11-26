using System;
using UnityEngine;

[Serializable]
public class LSceneObject : ScriptableObject
{
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

	internal void Init(AssetBundle bundle)
	{
		gameObject = bundle.LoadAsset<GameObject>(gameObjectAssetPath);
		//spawnCondition.Load();
		spawnCondition.Init(bundle);
	}

	public void Spawn(bool ignoreConditions = false)
	{
		if (!ignoreConditions && spawnCondition != null && !spawnCondition.Invoke(this))
			return;

		GameObject parent = GameObject.Find(parentTransform);
		Transform parentTr = parent == null ? null : parent.transform;
		GameObject clone = Instantiate(gameObject, localPosition, Quaternion.Euler(localRotation), parentTr);
		clone.transform.localScale = localScale;
	}

#if UNITY_EDITOR
	public void UpdatePath()
	{
		gameObjectAssetPath = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
	}
#endif
}