using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lunatic/Scene Object Group")]
public class LSceneObjectGroup : ScriptableObject, IModObject, ISerializationCallbackReceiver
{
	[Serializable]
    public class SceneObject : ISerializationCallbackReceiver
    {
        [JsonIgnore]
        public GameObject gameObject;

        public Vector3 localPosition;
        public Vector3 localRotation;
        public Vector3 localScale;
        public string parentTransform;

        [SerializeField, HideInInspector, JsonRequired]
        internal string gameObjectAssetPath;

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
            gameObjectAssetPath = UnityEditor.AssetDatabase.GetAssetPath(gameObject);
#endif
        }

		public void OnAfterDeserialize()
		{

		}
	}

    public Mod Mod { get; set; }
    public AssetBundle Bundle { get; set; }
    public string Name { get; set; }
    public string AssetName { get; set; }
    public string InternalName => Lunatic.GetInternalName(this);

	public string scene;
	public List<SceneObject> sceneObjects = new List<SceneObject>();

    public SceneObject Test;

	[SerializeField, HideInInspector]
    private string sceneObjectsJson;

    internal void Init()
	{
        sceneObjects = LJson.Deserialise<List<SceneObject>>(sceneObjectsJson);

        foreach (SceneObject sceneObject in sceneObjects)
        {
            Debug.Log($"Initing {sceneObject.gameObjectAssetPath}");
            sceneObject.gameObject = Bundle.LoadAsset<GameObject>(sceneObject.gameObjectAssetPath);
        }
	}

    public void Spawn()
    {
        foreach (SceneObject sceneObject in sceneObjects)
        {
            GameObject parent = GameObject.Find(sceneObject.parentTransform);
            Transform parentTr = parent == null ? null : parent.transform;
            GameObject gameObject = Instantiate(sceneObject.gameObject, sceneObject.localPosition, Quaternion.Euler(sceneObject.localRotation), parentTr);
            gameObject.transform.localScale = sceneObject.localScale;
		}
    }

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
        sceneObjectsJson = LJson.Serialise(sceneObjects);
#endif
	}

	public void OnAfterDeserialize()
	{

	}
}
