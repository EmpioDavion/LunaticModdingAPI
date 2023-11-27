using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lunatic/Scene Object Group")]
[System.Serializable]
public class LSceneObjectGroup : ScriptableObject, IModObject
{
    public Mod Mod { get; set; }
    public AssetBundle Bundle { get; set; }
    public string Name { get; set; }
    public string AssetName { get; set; }
    public string InternalName => Lunatic.GetInternalName(this);

	public string scene;
	public List<LSceneObject> sceneObjects = new List<LSceneObject>();

    [SerializeField]
    protected LSceneObjectGroupCondition spawnCondition;

	internal void Init()
	{
        spawnCondition.Init();
	}

    public void Spawn(bool ignoreConditions = false)
    {
        if (!ignoreConditions && spawnCondition != null && !spawnCondition.Invoke(this))
            return;

        foreach (LSceneObject sceneObject in sceneObjects)
            sceneObject.Spawn(ignoreConditions);
    }
}
