using UnityEngine;
using UnityEngine.SceneManagement;

// ModGame class that controls top level functionality for a mod
public class TemplateGame : ModGame
{
	// example mod data class to save/load
	// data that is stored should be types that will be able to be serialised to json
	public class MyModData
	{
		public int someIntValue;
		public float someFloatValue;
		public string someStringValue;
	}

	// store the data in a static variable so you can access it elsewhere if needed,
	// only one will ever be loaded anyway - the current save file
	public static MyModData ModData;

	// all your game assets should be stored in an AssetBundle
	// ensure that the assets you want in your mod are assigned to the AssetBundle,
	// you can do this by selecting an asset in Unity and assigning which,
	// AssetBundle to use in the dropdown that is in the bottom right of the screen
	// you can then build them by going to "Assets/Build AssetBundles" in the top menu
	public static AssetBundle MyAssets;

	// when a save file is loaded
	public override void OnSaveFileLoaded()
	{
		// Lunatic stores mod data in a file that is in the same folder as the Lunacid save file,
		// and will load data for all active mods when a Lunacid save file is loaded
		// the mod data will be stored as a string of text,
		// all you have to do is deserialise it
		ModData = Lunatic.GetModData<MyModData>();

		// keep in mind that if you create more than one ModGame script in your mod,
		// you will want only one of the scripts to serialise and deserialise data,
		// as calling GetModData creates a new object, and SetModData overwrites what is stored
		// data is only stored per mod, not per ModGame

		// if no data has been saved for this mod yet, setup the initial state for the mod
		if (ModData == null)
		{
			ModData = new MyModData
			{
				someIntValue = 1,
				someFloatValue = 3.0f,
				someStringValue = "Test"
			};
		}

		// if your mod is disabled, Lunatic will still load and save the json text,
		// so your mod save data won't disappear

		// Lunatic will load your AssetBundle in order to add your mod objects to the game
		// so you should retrieve your AssetBundle with GetAssetBundle as attempting to load
		// it a second time with AssetBundle.LoadFromFile will throw an error
		if (MyAssets == null)
			MyAssets = Lunatic.GetAssetBundle("modtemplate");
	}

	public override void OnSaveFileSaved()
	{
		// Lunatic is about to serialize the mod data to file, so update the json Lunatic has stored
		Lunatic.SetModData(ModData);
	}

	// changing from any scene to another scene
	// you can also use a ModScene script to track loading for a particular scene
	public override void OnSceneChange(Scene from, Scene to) { }

	// finished loading a scene
	public override void OnSceneLoaded(Scene from, Scene to) { }

	// here you can modify materials when a scene is loaded
	// this will run for each unique material in the scene
	// this won't run for objects that are spawned through scripts
	public override void EditMaterial(Material mat)
	{
		// replace Demi's face texture with our own
		if (mat.name == "demi_face_mat")
		{
			Texture2D texture = MyAssets.LoadAsset<Texture2D>("Demi_FaceLift");	// load our new texture
			mat.mainTexture = texture; // setting mainTexture is the same as mat.SetTexture("_MainTex", texture);
		}
	}
}
