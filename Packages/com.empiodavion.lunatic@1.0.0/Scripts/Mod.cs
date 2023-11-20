using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Lunatic/Mod Asset")]
public class Mod : ScriptableObject
{
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

	internal static int RecipesLoaded = 0;

	internal void Load(AssetBundle _bundle)
	{
		bundle = _bundle;

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
	}

	internal void Init()
	{
		Debug.Log("Initialising mod objects");

		InitWeapons();
		InitMagics();
		InitItems();
		InitMaterials();
		InitRecipes();
		InitClasses();

		foreach (ModRecipe modRecipe in recipes)
			Debug.Log($"{modRecipe.name} - {modRecipe.ingredient1.name}");
	}

	private void InitWeapons()
	{
		foreach (ModWeapon weapon in weapons)
		{
			Lunatic.TrackWeapon(weapon);
		}
	}

	private void InitMagics()
	{
		foreach (ModMagic magic in magics)
		{
			Lunatic.TrackMagic(magic);
		}
	}

	private void InitItems()
	{
		foreach (ModItem item in items)
		{
			Lunatic.TrackItem(item);
		}
	}

	private void InitMaterials()
	{
		foreach (ModMaterial material in materials)
		{
			material.id = Lunatic.MaterialNames.Count * 2 + 1;
			Lunatic.MaterialNames.Add(material.name);
		}
	}

	internal void InitRecipes()
	{
		foreach (ModRecipe modRecipe in recipes)
			modRecipe.Init();
	}

	internal void AddRecipes(Alki alki)
	{
		foreach (ModRecipe modRecipe in recipes)
		{
			Alki.Recipe recipe = CreateRecipe(modRecipe);

			Debug.Log($"Converting recipe: {modRecipe.AssetName} - {modRecipe.ingredient1.name}, {modRecipe.ingredient2.name}, {modRecipe.ingredient3.name}, ");
			Debug.Log($"Adding recipe: {recipe.name} - {recipe.need_1}, {recipe.need_2}, {recipe.need_3}");

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

	private Alki.Recipe CreateRecipe(ModRecipe modRecipe)
	{
		return new Alki.Recipe
		{
			name = modRecipe.name,
			des = modRecipe.description,
			unlocked = modRecipe.startsUnlocked ? 1 : 0,
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
