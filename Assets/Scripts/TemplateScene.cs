using UnityEngine.SceneManagement;

// ModScene class that controls scene level functionality for a mod
public class TemplateScene : ModScene
{
	// this class has a sceneName string value which indicates which scene it should track
	// you can see what the scene names are for the game at Lunatic.GameScenes

	private void Awake()
	{
		// set scene to Wing's Rest if sceneName was not set to anything
		if (string.IsNullOrEmpty(sceneName))
			sceneName = Lunatic.GameScenes.WingsRest;
	}

	// when this scene is entered
	public override void OnSceneLoaded(Scene oldScene)
	{

	}

	// when this scene is exited
	public override void OnSceneLeave(Scene newScene)
	{
		
	}
}
