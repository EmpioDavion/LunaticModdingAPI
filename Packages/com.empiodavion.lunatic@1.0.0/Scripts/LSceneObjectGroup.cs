using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lunatic/Scene Object Group")]
public class LSceneObjectGroup : ScriptableObject, IModObject, ISerializationCallbackReceiver
{
    public Mod Mod { get; set; }
    public AssetBundle Bundle { get; set; }
    public string Name { get; set; }
    public string AssetName { get; set; }
    public string InternalName => Lunatic.GetInternalName(this);

	public string scene;
	public List<LSceneObject> sceneObjects = new List<LSceneObject>();

	[SerializeField, HideInInspector, JsonProperty]
    internal string sceneObjectsJson;

    [SerializeField]
    protected LSceneObjectGroupCondition spawnCondition;

	internal void Init()
	{
        //spawnCondition.Print();
        //spawnCondition.Load();
        //spawnCondition.Reference.Init(Bundle);
        spawnCondition.Init(Bundle);

		//sceneObjects = LJson.Deserialise<List<SceneObject>>(sceneObjectsJson);

        //foreach (SceneObject sceneObject in sceneObjects)
        //    sceneObject.Init(Bundle);
	}

    public void Spawn(bool ignoreConditions = false)
    {
        if (!ignoreConditions && spawnCondition != null && !spawnCondition.Invoke(this))
            return;

        foreach (LSceneObject sceneObject in sceneObjects)
            sceneObject.Spawn(ignoreConditions);
    }

    public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
        foreach (LSceneObject sceneObject in sceneObjects)
            if (sceneObject != null)
                sceneObject.UpdatePath();

        //sceneObjectsJson = LJson.Serialise(sceneObjects);
#endif
	}

	public void OnAfterDeserialize()
	{

	}
}
