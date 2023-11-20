using UnityEngine.SceneManagement;

public class ModGame : UnityEngine.ScriptableObject, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string AssetName { get; set; }

	public virtual void OnSaveFileLoaded() { }

	public virtual void OnSaveFileSaved() { }

	public virtual void OnSceneChange(Scene from, Scene to) { }

	public virtual void OnSceneLoaded(Scene from, Scene to) { }

	public virtual void EditMaterial(UnityEngine.Material mat) { }
}
