using UnityEngine.SceneManagement;

public class ModScene : UnityEngine.ScriptableObject
{
	public string sceneName;

	public virtual void OnSceneLeave(Scene newScene)
	{

	}

	public virtual void OnSceneLoaded(Scene oldScene)
	{

	}
}
