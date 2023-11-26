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

	public readonly List<ModGame> games = new List<ModGame>();
	public readonly List<ModScene> scenes = new List<ModScene>();
	public readonly List<ModMultipleStates> npcs = new List<ModMultipleStates>();
	public readonly List<ModWeapon> weapons = new List<ModWeapon>();
	public readonly List<ModMagic> magics = new List<ModMagic>();
	public readonly List<ModItem> items = new List<ModItem>();
	public readonly List<ModMaterial> materials = new List<ModMaterial>();
	public readonly List<ModItemPickup> itemPickups = new List<ModItemPickup>();
	public readonly List<ModRecipe> recipes = new List<ModRecipe>();
	public readonly List<ModClass> classes = new List<ModClass>();
	public readonly List<LSceneObjectGroup> sceneObjectGroups = new List<LSceneObjectGroup>();

	internal static int RecipesLoaded = 0;

	private bool loaded = false;
	private bool initialised = false;

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
			IModObject modObject;

			if (asset is GameObject gameObject)
				modObject = gameObject.GetComponent<IModObject>();
			else
				modObject = asset as IModObject;

			if (modObject != null)
			{
				modObject.Mod = this;
				modObject.Bundle = bundle;
				modObject.AssetName = assetName;

				assets.Add(modObject);
			}
		}

		games.AddRange(assets.OfType<ModGame>());
		Debug.Log($"Added {games.Count} games.");

		scenes.AddRange(assets.OfType<ModScene>());
		Debug.Log($"Added {scenes.Count} scenes.");

		npcs.AddRange(assets.OfType<ModMultipleStates>());
		Debug.Log($"Added {npcs.Count} NPCS.");

		weapons.AddRange(assets.OfType<ModWeapon>());
		Debug.Log($"Added {weapons.Count} weapons.");

		magics.AddRange(assets.OfType<ModMagic>());
		Debug.Log($"Added {magics.Count} magics.");

		items.AddRange(assets.OfType<ModItem>());
		Debug.Log($"Added {items.Count} items.");

		materials.AddRange(assets.OfType<ModMaterial>());
		Debug.Log($"Added {materials.Count} materials.");

		itemPickups.AddRange(assets.OfType<ModItemPickup>());
		Debug.Log($"Added {itemPickups.Count} item pickups.");

		recipes.AddRange(assets.OfType<ModRecipe>());
		Debug.Log($"Added {recipes.Count} recipes.");

		classes.AddRange(assets.OfType<ModClass>());
		Debug.Log($"Added {classes.Count} classes.");

		sceneObjectGroups.AddRange(assets.OfType<LSceneObjectGroup>());
		Debug.Log($"Added {sceneObjectGroups.Count} scene object groups.");
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
		InitRecipes();
		InitClasses();
		InitSceneObjectGroups();
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
		}

		Lunatic.ModMagics.AddRange(magics);
	}

	private void InitItems()
	{
		foreach (ModItem item in items)
		{
			Lunatic.FixShaders(item);
			Lunatic.TrackItem(item);
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
