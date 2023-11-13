using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Lunatic
{
	public enum ItemTypes
	{
		Weapon,
		Magic,
		Gold,
		Item,
		Material
	}

	public static class GameScenes
	{
		public const string AccursedTomb = "HAUNT";
		public const string AHolyBattleground = "CAS_PITT";
		public const string BoilingGrotto = "CAVE";
		public const string CastleLeFanu = "CAS_1";
		public const string ChamberOfFate = "ARENA2";
		public const string CharacterCreation = "CHAR_CREATE";
		public const string EndingA = "END_A";
		public const string EndingB = "END_B";
		public const string EndingE = "END_EVID";
		public const string ForbiddenArchives = "ARCHIVES";
		public const string ForestCanopy = "FOREST_B1";
		public const string ForlornArena = "ARENA";
		public const string GameOver = "Gameover";
		public const string GraveOfTheSleeper = "END_TOWN";
		public const string GreatWellSurface = "PITT_B1";
		public const string HollowBasin = "PITT_A1";
		public const string LabyrinthOfAsh = "VOID";
		public const string LaetusChasm = "WALL_01";
		public const string MainMenu = "MainMenu";
		public const string PlayerDimension = "END_E";
		public const string SealedBallroom = "CAS_3";
		public const string TerminusPrison = "PRISON";
		public const string TheFetidMire = "SEWER_A1";
		public const string TheMausoleum = "HAUNT";
		public const string TheSanguineSea = "LAKE";
		public const string ThroneChamber = "CAS_2";
		public const string TowerOfAbyss = "TOWER";
		public const string WingsRest = "HUB_01";
		public const string YoseiForest = "FOREST_A1";

		public const string Unused = "WhatWillBeAtTheEnd";

		public static readonly Dictionary<string, string> NameToID = new Dictionary<string, string>()
		{
			{ nameof(AccursedTomb), AccursedTomb },
			{ nameof(AHolyBattleground), AHolyBattleground },
			{ nameof(BoilingGrotto), BoilingGrotto },
			{ nameof(CastleLeFanu), CastleLeFanu },
			{ nameof(ChamberOfFate), ChamberOfFate },
			{ nameof(CharacterCreation), CharacterCreation },
			{ nameof(EndingA), EndingA },
			{ nameof(EndingB), EndingB },
			{ nameof(EndingE), EndingE },
			{ nameof(ForbiddenArchives), ForbiddenArchives },
			{ nameof(ForestCanopy), ForestCanopy },
			{ nameof(ForlornArena), ForlornArena },
			{ nameof(GameOver), GameOver },
			{ nameof(GraveOfTheSleeper), GraveOfTheSleeper },
			{ nameof(GreatWellSurface), GreatWellSurface },
			{ nameof(HollowBasin), HollowBasin },
			{ nameof(LabyrinthOfAsh), LabyrinthOfAsh },
			{ nameof(LaetusChasm), LaetusChasm },
			{ nameof(MainMenu), MainMenu },
			{ nameof(PlayerDimension), PlayerDimension },
			{ nameof(SealedBallroom), SealedBallroom },
			{ nameof(TerminusPrison), TerminusPrison },
			{ nameof(TheFetidMire), TheFetidMire },
			{ nameof(TheMausoleum), TheMausoleum },
			{ nameof(TheSanguineSea), TheSanguineSea },
			{ nameof(ThroneChamber), ThroneChamber },
			{ nameof(TowerOfAbyss), TowerOfAbyss },
			{ nameof(WingsRest), WingsRest },
			{ nameof(YoseiForest), YoseiForest },

			{ nameof(Unused), Unused }
		};
	}

	public static class UIReferences
	{
		public static TextMeshProUGUI PlayerName { get; internal set; }
		public static TextMeshProUGUI PlayerTypedText { get; internal set; }

		public static GameObject PlayerResponseYes { get; internal set; }
		public static GameObject PlayerResponseNo { get; internal set; }
		public static GameObject PlayerResponseExit { get; internal set; }
	}

	public enum Elements
	{
		Normal,
		Fire,
		Ice,
		Poison,
		Light,
		Dark,
		DarkLight,
		NormalFire,
		IcePoison,
		DarkFire
	}

	public enum WeaponTypes
	{
		Melee,
		Ranged
	}

	public enum MagicTypes
	{
		Conjuring_Summoning,
		Aggressive_Damage,
		Force,
		Healing,
		Esotericism,
		Abjuration,
		Lunar
	}

	private static Dictionary<string, Object> AssetReplacement = new Dictionary<string, Object>();

	private static readonly List<Mod> Mods = new List<Mod>();

	public static readonly List<Renderer> Renderers = new List<Renderer>();
	public static readonly HashSet<Material> Materials = new HashSet<Material>();

	private static Scene LastScene;

	public static CONTROL Control { get; private set; }
	public static Player_Control_scr Player { get; private set; }

	// list of material item names
	public static string[] MaterialNames;

	private static Dictionary<string, string> ModData = new Dictionary<string, string>();

	private static bool Initialised = false;

	public static void Init()
	{
		if (Initialised)
			return;

		Initialised = true;

#if UNITY_EDITOR
		UnityEditor.PackageManager.PackageInfo package;
		package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Lunatic).Assembly);

		string txtPath = System.IO.Path.Combine(package.assetPath, "Lunacid\\Resources\\txt\\eng\\MATERIALS.txt");
		
		TextAsset materialTxt = AssetDatabase.LoadAssetAtPath<TextAsset>(txtPath);
#else
		SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
		SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;

		string lang = PlayerPrefs.GetString("LANG", "ENG");
		TextAsset materialTxt = Resources.Load<TextAsset>($"txt/{lang}/MATERIALS");
#endif

		List<string> materialNames = new List<string>();
		string[] names = materialTxt.text.Split('|');

		for (int i = 0; i < names.Length; i += 2)
			materialNames.Add(names[i].Replace("\r\n", ""));

		MaterialNames = materialNames.ToArray();
	}

	public static void AddMod(Mod mod)
	{
		mod.Init();
		Mods.Add(mod);
	}

	public static T GetModData<T>()
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();
		string name = assembly.GetName().Name;

		if (ModData.TryGetValue(name, out string json))
			return JsonUtility.FromJson<T>(json);

		return default;
	}

	public static void SetModData(object data)
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();
		string name = assembly.GetName().Name;

		ModData[name] = JsonUtility.ToJson(data);
	}

	private static void SceneManager_activeSceneChanged(Scene current, Scene next)
	{
		LastScene = current;

		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSceneChange(current, next);

		foreach (Mod mod in Mods)
			foreach (ModScene scene in mod.scenes)
				if (scene.name == current.name)
					scene.OnSceneLeave(next);
	}

	private static void SceneManager_sceneLoaded(Scene current, LoadSceneMode loadSceneMode)
	{
		Control = GetControl();
		Player = GetPlayer();

		UIReferences.PlayerName = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/NAME").GetComponent<TextMeshProUGUI>();
		UIReferences.PlayerTypedText = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/TYPED_TEXT").GetComponent<TextMeshProUGUI>();

		UIReferences.PlayerResponseYes = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/YES");
		UIReferences.PlayerResponseNo = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/NO");
		UIReferences.PlayerResponseExit = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/EXIT");

		Renderers.Clear();
		Renderers.AddRange(Object.FindObjectsOfType<Renderer>());

		Materials.Clear();

		foreach (Renderer renderer in Renderers)
		{
			Material[] materials = renderer.sharedMaterials;

			foreach (Material mat in materials)
				Materials.Add(mat);
		}

		foreach (Material mat in Materials)
			foreach (Mod mod in Mods)
				foreach (ModGame game in mod.games)
					game.EditMaterial(mat);

		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSceneLoaded(LastScene, current);

		foreach (Mod mod in Mods)
			foreach (ModScene scene in mod.scenes)
				if (scene.name == current.name)
					scene.OnSceneLoaded(LastScene);
	}

	public static AssetBundle LoadAssetBundle(string name)
	{
		System.Reflection.Assembly mod = System.Reflection.Assembly.GetCallingAssembly();
		string dir = System.IO.Path.GetDirectoryName(mod.Location);
		string path = System.IO.Path.Combine(dir, name);

		Debug.Log($"Loading asset bundle located at {path}");

		return AssetBundle.LoadFromFile(path);
	}

	internal static void TrackWeapon(ModWeapon weapon)
	{
		AssetReplacement.Add("WEPS/" + weapon.name, weapon.gameObject);
	}

	internal static void TrackMagic(ModMagic magic)
	{
		AssetReplacement.Add("MAGIC/" + magic.name, magic.gameObject);
	}

	internal static void TrackItem(ModItem item)
	{
		AssetReplacement.Add("ITEMS/" + item.name, item.gameObject);
	}

	internal static CONTROL GetControl()
	{
		GameObject controlGO = GameObject.Find("CONTROL");

		return controlGO.GetComponent<CONTROL>();
	}

	internal static Player_Control_scr GetPlayer()
	{
		GameObject playerGO = GameObject.Find("PLAYER");

		return playerGO.GetComponent<Player_Control_scr>();
	}

	private static void Give(string name, string[] array, string type)
	{
		int index = System.Array.FindIndex(array, (x) => string.IsNullOrEmpty(x));

		if (index >= 0)
			array[index] = name;
		else
			Debug.LogWarning($"Failed to find slot to add {type} {name}");
	}

	public static void GiveMagic(string name)
	{
		Player_Control_scr player = GetPlayer();
		Give(name, player.CON.CURRENT_PL_DATA.SPELLS, "magic");
	}

	public static void GiveWeapon(string name)
	{
		Player_Control_scr player = GetPlayer();
		Give(name, player.CON.CURRENT_PL_DATA.WEPS, "weapon");
	}

	public static void GiveItem(string name, int count)
	{
		Player_Control_scr player = GetPlayer();

		string eqName = name.Replace(' ', '_');
	 	Useable_Item item = System.Array.Find(player.CON.EQ_ITEMS, (x) => x.ITEM_NAME == eqName);

		if (item != null)
		{
			item.Count += 2;
			item.TakeOne();
		}

		string[] array = player.CON.CURRENT_PL_DATA.ITEMS;
		int index = System.Array.FindIndex(array, (x) => !string.IsNullOrEmpty(x) && StaticFuncs.REMOVE_NUMS(x) == name);

		if (index == -1)
			Give(name + count.ToString("00"), array, "item");
		else
		{
			int total = int.Parse(array[index].Substring(name.Length));
			total = System.Math.Min(total + count, 99);

			array[index] = name + total.ToString("00");
		}
	}

	// Patch functions

	public static void Internal_LoadModData(string file)
	{
		if (System.IO.File.Exists(file))
			ModData = JsonUtility.FromJson<Dictionary<string, string>>(System.IO.File.ReadAllText(file));

		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSaveFileLoaded();
	}

	public static void Internal_SaveModData(string file)
	{
		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSaveFileSaved();

		System.IO.File.WriteAllText(file, JsonUtility.ToJson(ModData));
	}

	public static bool ReplaceAsset(ref Object __result, string path)
	{
		if (AssetReplacement.TryGetValue(path, out Object obj))
		{
			__result = obj;

			return true;
		}

		return false;
	}

	public static void EditMaterial(ref Material mat)
	{
		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.EditMaterial(mat);
	}

	public static void OnLevelChange(string LVL)
	{
		Scene currentScene = SceneManager.GetActiveScene();
		Scene newScene = SceneManager.GetSceneByName(LVL);

		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSceneChange(currentScene, newScene);

		foreach (Mod mod in Mods)
		{
			foreach (ModGame game in mod.games)
			{
				ModScene scene = mod.scenes.Find((x) => x.name == currentScene.name);

				if (scene != null)
					scene.OnSceneLeave(newScene);
			}
		}
	}

	public static void OnPlayerJump(Player_Control_scr ___Player)
	{
		int index = System.Array.FindIndex(___Player.CON.CURRENT_PL_DATA.WEPS, (x) => string.IsNullOrEmpty(x));

		Debug.Log($"Adding to weapon index {index}");

		if (index >= 0)
			___Player.CON.CURRENT_PL_DATA.WEPS[index] = "TEST WEAPON";
	}

	public static void SetSkills(CONTROL __instance)
	{
		foreach (Mod mod in Mods)
			foreach (ModClass @class in mod.classes)
				if (__instance.CURRENT_PL_DATA.PLAYER_CLASS.ToUpper() == @class.name)
					@class.SetDamageMultipliers(__instance);
	}
}
