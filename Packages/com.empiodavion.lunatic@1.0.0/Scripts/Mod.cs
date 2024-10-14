using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Lunatic/Mod Asset")]
public class Mod : ScriptableObject
{
	[System.Serializable]
	public class Version
	{
		public int major;
		public int minor;
		public int build;
		public int revision;

		public Version()
		{
			major = 1;
			minor = 0;
			build = 0;
			revision = 0;
		}
	}

	public string Name;
	public Version version;

	[System.NonSerialized]
	public AssetBundle bundle;

	[System.NonSerialized]
	public ModLanguageSourceAsset localisation;

	public readonly List<ModGame> games = new List<ModGame>();
	public readonly List<ModScene> scenes = new List<ModScene>();
	public readonly List<ModMultipleStates> npcs = new List<ModMultipleStates>();
	public readonly List<ModWeapon> weapons = new List<ModWeapon>();
	public readonly List<ModMagic> magics = new List<ModMagic>();
	public readonly List<ModItem> items = new List<ModItem>();
	public readonly List<ModMaterial> materials = new List<ModMaterial>();
	public readonly List<ModItemPickup> itemPickups = new List<ModItemPickup>();
	public readonly List<ModProjectile> projectiles = new List<ModProjectile>();
	public readonly List<ModRecipe> recipes = new List<ModRecipe>();
	public readonly List<ModClass> classes = new List<ModClass>();
	public readonly List<LSceneObjectGroup> sceneObjectGroups = new List<LSceneObjectGroup>();
	public readonly List<LSceneObject> sceneObjects = new List<LSceneObject>();
	public readonly List<LConditionBase> conditions = new List<LConditionBase>();

	public readonly Dictionary<string, int> npcStates = new Dictionary<string, int>();

	internal static int RecipesLoaded = 0;

	private bool loaded = false;
	private bool initialised = false;

	private void InitModObject(IModObject modObject, string assetName)
	{
		modObject.Mod = this;
		modObject.Bundle = bundle;
		modObject.AssetName = assetName;
	}

	internal void Load()
	{
		if (loaded)
			return;

		loaded = true;

		string[] assetNames = bundle.GetAllAssetNames();
		List<IModObject> assets = new List<IModObject>();

		foreach (string assetName in assetNames)
		{
			Object asset = bundle.LoadAsset(assetName);
			
			if (asset is GameObject gameObject)
			{
				IModObject[] modObjects = gameObject.GetComponentsInChildren<IModObject>(true);

				foreach (IModObject modObject in modObjects)
				{
					InitModObject(modObject, assetName);
					assets.Add(modObject);
				}
			}
			else
			{
				if (asset is IModObject modObject)
				{
					InitModObject(modObject, assetName);
					assets.Add(modObject);
				}
			}
		}

		int locIndex = System.Array.IndexOf(assetNames, "assets/localisation.asset");

		if (locIndex >= 0)
		{
			Debug.Log($"Found localisation asset");

			localisation = (ModLanguageSourceAsset)assets[locIndex];
		}
		else
			Debug.LogWarning("Could not find localisation asset.");

		Load(games, assets, "games");
		Load(scenes, assets, "scenes");
		Load(npcs, assets, "NPCS");
		Load(weapons, assets, "weapons");
		Load(magics, assets, "magics");
		Load(items, assets, "items");
		Load(materials, assets, "materials");
		Load(itemPickups, assets, "item pickups");
		Load(projectiles, assets, "projectiles");
		Load(recipes, assets, "recipes");
		Load(classes, assets, "classes");
		Load(sceneObjectGroups, assets, "scene object groups");
		Load(sceneObjects, assets, "scene objects");
		Load(conditions, assets, "conditions");

		// npc states are loaded from save files
	}

	private void Load<T>(List<T> list, List<IModObject> assets, string typeName)
	{
		list.AddRange(assets.OfType<T>());
		Debug.Log($"Added {list.Count} {typeName}.");

	}

	internal void Init()
	{
		if (initialised)
			return;

		initialised = true;

		InitWeapons();
		InitMagics();
		InitItems();
		InitMaterials();
		InitItemPickups();
		InitProjectiles();
		InitRecipes();
		InitClasses();
		InitSceneObjectGroups();
		InitSceneObjects();
		InitConditions();
	}

	private void InitWeapons()
	{
		foreach (ModWeapon weapon in weapons)
		{
			Lunatic.FixShaders(weapon);
			Lunatic.TrackWeapon(weapon);
		}

		Lunatic.ModWeapons.AddRange(weapons);
	}

	private void InitMagics()
	{
		foreach (ModMagic magic in magics)
		{
			Lunatic.FixShaders(magic);
			Lunatic.TrackMagic(magic);
			magic.Init();
		}

		Lunatic.ModMagics.AddRange(magics);
	}

	private void InitItems()
	{
		foreach (ModItem item in items)
		{
			Lunatic.FixShaders(item);
			Lunatic.TrackItem(item);
			item.Init();
		}

		Lunatic.ModItems.AddRange(items);
	}

	private void InitMaterials()
	{
		foreach (ModMaterial material in materials)
		{
			material.id = Lunatic.MaterialNames.Count * 2 + 1;
			Lunatic.MaterialNames.Add(material.InternalName);
		}

		Lunatic.ModMaterials.AddRange(materials);
	}

	private void InitItemPickups()
	{
		foreach (ModItemPickup itemPickup in itemPickups)
		{
			Lunatic.FixShaders(itemPickup);
			itemPickup.Init();
		}
	}

	private void InitProjectiles()
	{
		foreach (ModProjectile projectile in projectiles)
		{
			Lunatic.FixShaders(projectile);
			Lunatic.TrackProjectile(projectile);
		}
	}

	internal void InitRecipes()
	{
		foreach (ModRecipe modRecipe in recipes)
			modRecipe.Init();

		Lunatic.ModRecipes.AddRange(recipes);
	}

	internal void AddRecipes(Alki alki)
	{
		foreach (ModRecipe modRecipe in recipes)
		{
			Alki.Recipe recipe = CreateRecipe(modRecipe);

			modRecipe.recipeIndex = RecipesLoaded;
			alki.Recipes[RecipesLoaded++] = recipe;
		}
	}

	private void InitClasses()
	{
		if (classes.Count > 0)
		{
			Menus.CharClass[] curClasses = Lunatic.Control.MENU.Classes;
			int curSize = curClasses.Length;
			int newSize = curSize + classes.Count;
			Menus.CharClass[] newClasses = new Menus.CharClass[newSize];
			curClasses.CopyTo(newClasses, 0);

			for (int i = 0; i < classes.Count; i++)
				newClasses[curSize + i] = CreateClass(classes[i]);

			Lunatic.Control.MENU.Classes = newClasses;
		}
	}

	private void InitSceneObjectGroups()
	{
		foreach (LSceneObjectGroup sceneObjectGroup in sceneObjectGroups)
			sceneObjectGroup.Init();
	}

	private void InitSceneObjects()
	{
		foreach (LSceneObject sceneObject in sceneObjects)
			sceneObject.Init();
	}

	private void InitConditions()
	{
		foreach (LConditionBase condition in conditions)
			condition.Init();
	}

	private Alki.Recipe CreateRecipe(ModRecipe modRecipe)
	{
		return new Alki.Recipe
		{
			name = modRecipe.name,
			des = modRecipe.description,
			unlocked = (modRecipe.startsUnlocked || modRecipe.isUnlocked) ? 1 : 0,
			need_1 = modRecipe.ingredient1.GetID(),
			need_2 = modRecipe.ingredient2.GetID(),
			need_3 = modRecipe.ingredient3.GetID(),
			SPAWN = modRecipe.result
		};
	}

	private Menus.CharClass CreateClass(ModClass modClass)
	{
		return new Menus.CharClass
		{
			name = modClass.name,
			desc = modClass.description,
			LVL = modClass.level,
			STR = modClass.strength,
			DEF = modClass.defense,
			SPD = modClass.speed,
			DEX = modClass.dexterity,
			INT = modClass.intelligence,
			RES = modClass.resistance
		};
	}
}
