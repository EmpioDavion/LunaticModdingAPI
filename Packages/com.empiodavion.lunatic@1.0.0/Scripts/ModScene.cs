using UnityEngine.SceneManagement;

public class ModScene : UnityEngine.ScriptableObject, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public string sceneName;

	public virtual void OnSceneLeave(Scene newScene)
	{

	}

	public virtual void OnSceneLoaded(Scene oldScene)
	{

	}
}
