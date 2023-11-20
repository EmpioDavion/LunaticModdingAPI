using System.Collections.Generic;
using TMPro;
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

	internal struct ModMaterialSaveData
	{
		public string name;
		public int count;

		public override string ToString()
		{
			int id = GetMaterialID(name);
			
			return id + count.ToString("00");
		}
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

	public enum DevMagicEffects
	{
		Gain100XP,
		ResetPlayerLevel,
		ToggleGodMode,
		MoveForward
	}

	private static readonly Dictionary<string, Object> AssetReplacement = new Dictionary<string, Object>();

	private static readonly List<Mod> Mods = new List<Mod>();

	public static readonly List<Renderer> Renderers = new List<Renderer>();
	public static readonly HashSet<Material> Materials = new HashSet<Material>();

	private static Scene LastScene;

	public static CONTROL Control { get; private set; }
	public static Player_Control_scr Player { get; private set; }

	// list of material item names
	public static readonly List<string> MaterialNames = new List<string>();

	private static Dictionary<string, string> ModData = new Dictionary<string, string>();

	private static readonly Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();

	private static readonly Dictionary<int, Alki> AlchemyTables = new Dictionary<int, Alki>();

	private static bool Initialised = false;
	private static bool ModsLoaded = false;

	private static int BaseMaterialCount;

	private static AssetBundle Bundle;

	public static void Init()
	{
		if (Initialised)
			return;

		Initialised = true;

#if UNITY_EDITOR
		
		UnityEditor.PackageManager.PackageInfo package;
		package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Lunatic).Assembly);

		string txtPath = System.IO.Path.Combine(package.assetPath, "Lunacid\\Resources\\txt\\eng\\MATERIALS.txt");

		TextAsset materialTxt = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(txtPath);

#else
		SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
		SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
		SceneManager.sceneLoaded += SceneManager_sceneLoaded;

		string lang = PlayerPrefs.GetString("LANG", "ENG");
		TextAsset materialTxt = Resources.Load<TextAsset>($"txt/{lang}/MATERIALS");
#endif

		MaterialNames.Clear();
		string[] names = materialTxt.text.Split('|');

		for (int i = 0; i < names.Length - 1; i += 2)
			MaterialNames.Add(names[i].Replace("\r\n", ""));

		BaseMaterialCount = MaterialNames.Count;
	}

	private static void LoadMods()
	{
		if (ModsLoaded)
			return;

		ModsLoaded = true;

		Control = GetControl();

		Debug.Log("Searching for dlls");

		string[] dlls = System.IO.Directory.GetFiles("BepInEx/plugins/", "*.dll", System.IO.SearchOption.AllDirectories);

		// load assemblies before loading asset bundles to ensure script types are loaded
		foreach (string dll in dlls)
			System.Reflection.Assembly.LoadFile(dll);

		string[] manifests = System.IO.Directory.GetFiles("BepInEx/plugins/", "*.manifest", System.IO.SearchOption.AllDirectories);
		
		Debug.Log($"Loading {manifests.Length} Lunatic mod(s)...");

		foreach (string manifest in manifests)
		{
			Debug.Log("Found manifest " + manifest);

			string bundlePath = manifest.Substring(0, manifest.LastIndexOf('.'));

			try
			{
				Debug.Log("Loading AssetBundle " + bundlePath);

				AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
				string[] assetNames = bundle.GetAllAssetNames();
				Mod[] mods = bundle.LoadAllAssets<Mod>();

				AssetBundles.Add(bundle.name, bundle);

				if (mods.Length == 0)
				{
					Debug.LogError("AssetBundle contains no Mod object, adding default Mod object for it.");
					Mod mod = ScriptableObject.CreateInstance<Mod>();
					mod.name = System.IO.Path.GetFileName(bundlePath);

					AddMod(mod, bundle);
					mod.Init();
				}
				else
				{
					// add mods, load assets from bundles and count totals
					foreach (Mod mod in mods)
					{
						Debug.Log("Loading mod assets " + mod.name);

						AddMod(mod, bundle);
					}

					foreach (Mod mod in mods)
					{
						Debug.Log("Initialising mod " + mod.name);

						mod.Init();
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
			}
		}

		Bundle = AssetBundles["lunatic"];
	}

	private static void AddMod(Mod mod, AssetBundle bundle)
	{
		mod.Load(bundle);
		Mods.Add(mod);
	}

	// due to truncation, don't need to bother with checking whether it's a mod material or not
	public static string GetMaterialName(int id) => MaterialNames[id / 2];

	public static int GetMaterialID(string name)
	{
		int index = MaterialNames.IndexOf(name);
		int id = index * 2;

		// there is an extra empty string at MaterialNames[BaseMaterialCount - 1]
		if (index >= BaseMaterialCount)
			id++;

		return id;
	}

	public static void Internal_InitRecipesArray(Alki alki)
	{
		int id = alki.GetInstanceID();

		// alchemy instance already initialised
		if (AlchemyTables.ContainsKey(id))
			return;

		AlchemyTables[id] = alki;

		int recipeCount = 0;
		Mods.ForEach((x) => recipeCount += x.recipes.Count);

		Mod.RecipesLoaded = alki.Recipes.Length;

		if (recipeCount > 0)
		{
			Alki.Recipe[] recipes = new Alki.Recipe[alki.Recipes.Length + recipeCount];
			alki.Recipes.CopyTo(recipes, 0);

			alki.Recipes = recipes;

			foreach (Mod mod in Mods)
				mod.AddRecipes(alki);
		}
	}

	public static T GetModData<T>(Mod mod)
	{
		if (ModData.TryGetValue(mod.name, out string json))
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
		AlchemyTables.Clear();

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
		Debug.Log("Loaded scene " + current.name);

		Control = GetControl();
		Player = GetPlayer();

		UIReferences.PlayerName = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/NAME").GetComponent<TextMeshProUGUI>();
		UIReferences.PlayerTypedText = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/TYPED_TEXT").GetComponent<TextMeshProUGUI>();

		UIReferences.PlayerResponseYes = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/YES");
		UIReferences.PlayerResponseNo = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/NO");
		UIReferences.PlayerResponseExit = GameObject.Find("PLAYER/Canvas/HUD/DIALOG/EXIT");

		LoadMods();

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
				if (scene.sceneName == current.name)
					scene.OnSceneLoaded(LastScene);
	}

	public static AssetBundle GetAssetBundle()
	{
		System.Reflection.Assembly mod = System.Reflection.Assembly.GetCallingAssembly();
		string dir = System.IO.Path.GetDirectoryName(mod.Location);
		string name = System.IO.Path.GetFileNameWithoutExtension(mod.Location);
		string path = System.IO.Path.Combine(dir, name);

		Debug.Log($"Loading asset bundle located at {path}");

		if (AssetBundles.TryGetValue(name, out AssetBundle bundle))
			return bundle;
		else
			Debug.LogError("Could not find AssetBundle " + name);

		return null;
	}

	public static AssetBundle GetAssetBundle(string name)
	{
		Debug.Log($"Grabbing asset bundle named {name}");

		if (AssetBundles.TryGetValue(name, out AssetBundle bundle))
			return bundle;
		else
			Debug.LogError("Could not find AssetBundle " + name);

		return null;
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

		return playerGO.GetComponentInChildren<Player_Control_scr>();
	}

	private static void Give(string name, string[] array, string type, int index = -1)
	{
		Debug.Log($"Giving {type} {name} to player");

		if (index == -1)
			index = System.Array.FindIndex(array, (x) => string.IsNullOrEmpty(x));

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
			Give(name + count.ToString("00"), array, "item", index);
		else
		{
			int total = int.Parse(array[index].Substring(name.Length));
			total = System.Math.Min(total + count, 99);

			array[index] = name + total.ToString("00");
		}
	}

	public static void GiveMaterial(string name, int count)
	{
		Player_Control_scr player = GetPlayer();

		string[] array = player.CON.CURRENT_PL_DATA.MATER;
		int index = System.Array.FindIndex(array, (x) => !string.IsNullOrEmpty(x) && x.Substring(0, x.Length - 2) == name);

		if (index == -1)
		{
			int id = GetMaterialID(name);

			if (id != -1)
			{
				name = id.ToString();
				index = System.Array.FindIndex(array, (x) => !string.IsNullOrEmpty(x) && x.Substring(0, x.Length - 2) == name);
			}
		}

		if (index == -1)
			Give(name + count.ToString("00"), array, "material", index);
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

	public static PlayerData Internal_OnPlayerDataLoad(PlayerData playerData)
	{
		if (ModData.TryGetValue("Lunatic", out string json))
		{
			List<ModMaterialSaveData> modMaterials = JsonUtility.FromJson<List<ModMaterialSaveData>>(json);

			int index = System.Array.FindIndex(playerData.MATER, string.IsNullOrEmpty);

			if (index >= 0)
			{
				foreach (ModMaterialSaveData modMaterial in modMaterials)
				{
					if (index >= playerData.MATER.Length)
						break;

					playerData.MATER[index++] = modMaterial.ToString();
				}
			}
		}

		return playerData;
	}

	public static void Internal_OnPlayerDataSave(PlayerData playerData)
	{
		// TODO: remove mod objects
		//List<string> weapons = new List<string>(playerData.WEPS.Length);
		List<string> materials = new List<string>(playerData.MATER);
		List<ModMaterialSaveData> modMaterials = new List<ModMaterialSaveData>();

		for (int i = 0; i < materials.Count; i++)
		{
			if (string.IsNullOrEmpty(materials[i]))
				break;

			int id = int.Parse(materials[i].Substring(0, materials[i].Length - 2));

			if (id > BaseMaterialCount * 2)
			{
				materials[i] = "";

				modMaterials.Add(new ModMaterialSaveData
				{
					name = GetMaterialName(id),
					count = int.Parse(materials[i].Substring(materials[i].Length - 2))
				});
			}
		}

		// send empty strings to end of list
		materials.Sort(PushBlankToEnd);

		playerData.MATER = materials.ToArray();

		SetModData(modMaterials);
	}

	private static int PushBlankToEnd(string a, string b)
	{
		if (string.IsNullOrEmpty(a))
		{
			if (string.IsNullOrEmpty(b))
				return 0;

			return 1;
		}
		else if (string.IsNullOrEmpty(b))
			return -1;

		return 0;
	}

	public static void Internal_AddMaterialTexts(Menus menus)
	{
		int materialCount = 0;

		Mods.ForEach((x) => materialCount += x.materials.Count);

		if (materialCount > 0)
		{
			string[] texts = new string[menus.MATTER_TXT.Length + materialCount * 2];
			menus.MATTER_TXT.CopyTo(texts, 0);

			int index = menus.MATTER_TXT.Length;

			foreach (Mod mod in Mods)
			{
				foreach (ModMaterial material in mod.materials)
				{
					texts[index++] = material.name;
					texts[index++] = material.description;
				}
			}

			menus.MATTER_TXT = texts;
		}
	}

	public static bool Internal_ReplaceAsset(ref Object __result, string path)
	{
		Debug.Log("Loading resource " + path);

		if (AssetReplacement.TryGetValue(path, out Object obj))
		{
			__result = obj;

			return true;
		}

		return false;
	}

	public static void Internal_CheckAsset(ref Object __result, string path)
	{
		if (__result == null)
		{
			int slash = path.IndexOf('/');

			if (slash == -1)
				return;

			string folder = path.Substring(0, slash);
			string replacement;

			switch (folder)
			{
				case "WEPS": replacement = "BASE WEAPON"; break;
				case "MAGIC": replacement = "BASE MAGIC"; break;
				case "ITEMS": replacement = "BASE ITEM"; break;
				default: return;
			}

			Debug.LogWarning($"Could not find {path}, replacing with {replacement}");
			__result = Bundle.LoadAsset(replacement);
		}
	}

	public static void Internal_EditMaterial(ref Material mat)
	{
		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.EditMaterial(mat);
	}

	public static void Internal_OnLevelChange(string LVL)
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

	public static void Internal_OnPlayerJump(Player_Control_scr ___Player)
	{
		GiveMaterial("TEMPLATE MATERIAL", 2);
	}

	public static void Internal_SetSkills(CONTROL __instance)
	{
		foreach (Mod mod in Mods)
			foreach (ModClass modClass in mod.classes)
				if (__instance.CURRENT_PL_DATA.PLAYER_CLASS.ToUpper() == modClass.name)
					modClass.SetDamageMultipliers(__instance);
	}
}
