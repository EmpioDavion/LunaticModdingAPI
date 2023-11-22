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

	internal static readonly List<Mod> Mods = new List<Mod>();

	public static readonly List<Renderer> Renderers = new List<Renderer>();
	public static readonly HashSet<Material> Materials = new HashSet<Material>();

	private static Scene LastScene;

	public static CONTROL Control { get; private set; }
	public static Player_Control_scr Player { get; private set; }

	// list of material item names
	public static readonly List<string> MaterialNames = new List<string>();

	internal static readonly List<ModWeapon> ModWeapons = new List<ModWeapon>();
	internal static readonly List<ModMagic> ModMagics = new List<ModMagic>();
	internal static readonly List<ModItem> ModItems = new List<ModItem>();
	internal static readonly List<ModMaterial> ModMaterials = new List<ModMaterial>();
	internal static readonly List<ModRecipe> ModRecipes = new List<ModRecipe>();

	private static Dictionary<string, string> ModData = new Dictionary<string, string>();

	private static readonly Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();

	private static readonly Dictionary<int, Alki> AlchemyTables = new Dictionary<int, Alki>();

	private static readonly Dictionary<string, Shader> LunacidShaders = new Dictionary<string, Shader>();

	private static bool Initialised = false;
	private static bool ModsLoaded = false;

	public static int BaseMaterialCount { get; private set; }

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
		SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
		SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;

		string lang = PlayerPrefs.GetString("LANG", "ENG");
		TextAsset materialTxt = Resources.Load<TextAsset>($"txt/{lang}/MATERIALS");
#endif

		MaterialNames.Clear();
		string[] names = materialTxt.text.Split('|');

		for (int i = 0; i < names.Length - 1; i += 2)
			MaterialNames.Add(names[i].Replace("\r\n", ""));

		BaseMaterialCount = MaterialNames.Count;

		AddShader("Lunacid/Mix");
		AddShader("Lunacid/Unlit Transparent No Fog");
		AddShader("ProBuilder/Standard Vertex Color");
		AddShader("ProBuilder/Unlit Vertex Color");
		AddShader("Retro Look Pro/Analog TV Noise");
		AddShader("Retro Look Pro/Bleed Effect");
		AddShader("Retro Look Pro/Bottom Noise Effect");
		AddShader("Retro Look Pro/Glitch 1 Retro Look");
		AddShader("Retro Look Pro/Glitch 2 Retro Look");
		AddShader("Retro Look Pro/Glitch 3");
		AddShader("Retro Look Pro/Jitter Effect");
		AddShader("Retro Look Pro/Low Res_RL Pro");
		AddShader("Retro Look Pro/Noise");
		AddShader("Retro Look Pro/NTSC_RL Pro");
		AddShader("Retro Look Pro/Phosphor_RL Pro");
		AddShader("Retro Look Pro/Picture Correction");
		AddShader("Retro Look Pro/TV_Retro Look");
		AddShader("Retro Look Pro/VHS_Retro Look");
		AddShader("Retro Look Pro/VHS Scanlines_RL Pro");
		AddShader("Shader Forge/Additive_Ignore_Fog");
		AddShader("Shader Forge/CASMOON");
		AddShader("Shader Forge/Clouds");
		AddShader("Shader Forge/Fade to Image");
		AddShader("Shader Forge/Fairy");
		AddShader("Shader Forge/Fake_Caustics");
		AddShader("Shader Forge/Fish_Shader");
		AddShader("Shader Forge/flame_bill");
		AddShader("Shader Forge/Gamma");
		AddShader("Shader Forge/ghost");
		AddShader("Shader Forge/Ghost Dark");
		AddShader("Shader Forge/Glow Wave");
		AddShader("Shader Forge/Hair");
		AddShader("Shader Forge/Heatwave");
		AddShader("Shader Forge/Ignore_Fog");
		AddShader("Shader Forge/ITEM_FLARE");
		AddShader("Shader Forge/Lava");
		AddShader("Shader Forge/Light_Beam");
		AddShader("Shader Forge/Light_Spot");
		AddShader("Shader Forge/Lit_Vertex");
		AddShader("Shader Forge/MIGO_shader");
		AddShader("Shader Forge/MLGS");
		AddShader("Shader Forge/Moon_Reflection");
		AddShader("Shader Forge/Moon_shader");
		AddShader("Shader Forge/New_Water_super");
		AddShader("Shader Forge/Object");
		AddShader("Shader Forge/Object_Clip");
		AddShader("Shader Forge/Object_DBL");
		AddShader("Shader Forge/Object_glow");
		AddShader("Shader Forge/Oil");
		AddShader("Shader Forge/Particle_Ignore_Fog");
		AddShader("Shader Forge/Radiant");
		AddShader("Shader Forge/Reflections");
		AddShader("Shader Forge/Shiney Surf");
		AddShader("Shader Forge/swim_shady");
		AddShader("Shader Forge/Unlit_Object");
		AddShader("Shader Forge/Water");
		AddShader("Shader Forge/Water_alt");
		AddShader("Shader Forge/Water_Flow");
		AddShader("Shader Forge/Water_SWirl");
		AddShader("Shader Forge/Wind");
		AddShader("Shader Forge/Winder");
		AddShader("Volumetric Cloud 3");

		foreach (KeyValuePair<string, Shader> kvp in LunacidShaders)
			if (kvp.Value == null)
				Debug.Log($"Shader \"{kvp.Key}\" is null");
	}

	private static void AddShader(string shaderPath)
	{
		LunacidShaders.Add(shaderPath, Shader.Find(shaderPath));
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

	public static ModWeapon GetModWeapon(string name)
	{
		return ModWeapons.Find((x) => x.name == name);
	}

	public static ModMagic GetModMagic(string name)
	{
		return ModMagics.Find((x) => x.name == name);
	}

	public static ModItem GetModItem(string name)
	{
		return ModItems.Find((x) => x.name == name);
	}

	public static ModMaterial GetModMaterial(string name)
	{
		return ModMaterials.Find((x) => x.name == name);
	}

	public static ModMaterial GetModMaterial(int id)
	{
		int index = id / 2 - BaseMaterialCount;
		return ModMaterials[index];
	}

	public static int GetMaterialID(string name)
	{
		int index = MaterialNames.IndexOf(name);
		int id = index * 2;

		// there is an extra empty string at MaterialNames[BaseMaterialCount - 1]
		if (index >= BaseMaterialCount)
			id++;

		return id;
	}

	internal static string GetInternalName(IModObject modObject)
	{
		return $"L#{modObject.Mod.name}/{modObject.Name}";
	}

	public static void ReadInternalName(string internalName, out string modName, out string objectName, bool hasData)
	{
		int slash = internalName.IndexOf('/', 3);
		modName = internalName.Substring(2, slash - 3);
		objectName = internalName.Substring(slash + 1, internalName.Length - slash - (hasData ? 3 : 1));
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

	public static T GetModData<T>()
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();
		string name = assembly.GetName().Name;

		if (ModData.TryGetValue(name, out string json))
			return LJson.Deserialise<T>(json);

		return default;
	}

	public static void SetModData(object data)
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();
		string name = assembly.GetName().Name;

		ModData[name] = LJson.Serialise(data);
	}

	private static void SceneManager_activeSceneChanged(Scene current, Scene next)
	{
		AlchemyTables.Clear();

		LastScene = current;

		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSceneChange(current, next);
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

		foreach (Mod mod in Mods)
			foreach (LSceneObjectGroup sceneObjectGroup in mod.sceneObjectGroups)
				if (sceneObjectGroup.scene == current.name)
					sceneObjectGroup.Spawn();

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

	private static void SceneManager_sceneUnloaded(Scene current)
	{
		foreach (Mod mod in Mods)
			foreach (ModScene scene in mod.scenes)
				if (scene.name == current.name)
					scene.OnSceneLeave(current);
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

	internal static void FixShaders(Component component)
	{
		Renderer[] renderers = component.GetComponentsInChildren<Renderer>();

		foreach (Renderer renderer in renderers)
		{
			Material[] materials = renderer.sharedMaterials;

			foreach (Material material in materials)
			{
				if (material == null)
					continue;

				if (LunacidShaders.TryGetValue(material.shader.name, out Shader shader))
					material.shader = shader;
			}

			renderer.sharedMaterials = materials;
		}
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
			ModData = LJson.Deserialise<Dictionary<string, string>>(System.IO.File.ReadAllText(file));

		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSaveFileLoaded();
	}

	public static void Internal_SaveModData(string file)
	{
		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSaveFileSaved();

		System.IO.File.WriteAllText(file, LJson.Serialise(ModData));
	}

	public static PlayerData Internal_OnPlayerDataLoad(PlayerData playerData)
	{
		LPlayerData data = GetModData<LPlayerData>();

		if (data != null)
			LPlayerData.Load(data, playerData);

		return playerData;
	}

	public static void Internal_OnPlayerDataSave(PlayerData playerData)
	{
		// TODO: remove mod objects
		LPlayerData data = LPlayerData.Save(playerData);

		SetModData(data);
	}

	public static bool Internal_OnItemPickupStart(Item_Pickup_scr itemPickup)
	{
		if (itemPickup is ModItemPickup modItemPickup)
			return modItemPickup.Internal_CheckStart();

		return false;
	}

	public static void SortWeapons(List<string> list)
	{
		list.Sort(Internal_SortWeapon);
	}

	internal static int Internal_SortWeapon(string a, string b)
	{
		if (a.StartsWith("L#"))
			a = a.Substring(a.IndexOf('/', 3) + 1);

		if (b.StartsWith("L#"))
			b = b.Substring(b.IndexOf('/', 3) + 1);

		return a.CompareTo(b);
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
		{
			foreach (ModGame game in mod.games)
			{
				ModScene scene = mod.scenes.Find((x) => x.name == currentScene.name);

				if (scene != null)
					scene.OnSceneLeave(newScene);
			}
		}

		foreach (Mod mod in Mods)
			foreach (ModGame game in mod.games)
				game.OnSceneChange(currentScene, newScene);
	}

	public static void Internal_OnPlayerJump(Player_Control_scr player)
	{

	}

	public static void Internal_SetSkills(CONTROL control)
	{
		foreach (Mod mod in Mods)
			foreach (ModClass modClass in mod.classes)
				if (control.CURRENT_PL_DATA.PLAYER_CLASS.ToUpper() == modClass.name)
					modClass.SetDamageMultipliers(control);
	}

	public static bool Internal_CheckIngredientCounts(Menus menus, int materialIndex, int EQ_SLOT)
	{
		string material = menus.CON.CURRENT_PL_DATA.MATER[materialIndex];
		int materialID = int.Parse(material.Substring(0, material.Length - 2));
		int materialCount = int.Parse(material.Substring(material.Length - 2));
		int needed = 1;

		if (EQ_SLOT != 0 && menus.ALKI.current_1 == materialID)
			needed++;

		if (EQ_SLOT != 1 && menus.ALKI.current_2 == materialID)
			needed++;

		if (EQ_SLOT != 2 && menus.ALKI.current_3 == materialID)
			needed++;

		Debug.Log($"ID: {materialID}, Count: {materialCount}, Needed: {needed}");

		return materialCount >= needed;
	}

	public static void RemoveUnavailableIngredients(Alki alki)
	{

	}
}
