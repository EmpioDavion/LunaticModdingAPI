using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lunatic/Mod Asset")]
public class Mod : ScriptableObject
{
	[System.NonSerialized]
	public AssetBundle bundle;

	public readonly List<ModGame> games = new List<ModGame>();
	public readonly List<ModScene> scenes = new List<ModScene>();
	public readonly List<ModWeapon> weapons = new List<ModWeapon>();
	public readonly List<ModMagic> magics = new List<ModMagic>();
	public readonly List<ModItem> items = new List<ModItem>();
	public readonly List<ModMaterial> materials = new List<ModMaterial>();
	public readonly List<ModRecipe> recipes = new List<ModRecipe>();
	public readonly List<ModClass> classes = new List<ModClass>();

	internal void Init(AssetBundle _bundle)
	{
		bundle = _bundle;

		games.AddRange(bundle.LoadAllAssets<ModGame>());
		scenes.AddRange(bundle.LoadAllAssets<ModScene>());
		weapons.AddRange(bundle.LoadAllAssets<ModWeapon>());
		magics.AddRange(bundle.LoadAllAssets<ModMagic>());
		items.AddRange(bundle.LoadAllAssets<ModItem>());
		materials.AddRange(bundle.LoadAllAssets<ModMaterial>());
		recipes.AddRange(bundle.LoadAllAssets<ModRecipe>());
		classes.AddRange(bundle.LoadAllAssets<ModClass>());

		Debug.Log($"Added {games.Count} games.");
		Debug.Log($"Added {scenes.Count} scenes.");
		Debug.Log($"Added {weapons.Count} weapons.");
		Debug.Log($"Added {magics.Count} magics.");
		Debug.Log($"Added {items.Count} items.");
		Debug.Log($"Added {materials.Count} materials.");
		Debug.Log($"Added {recipes.Count} recipes.");
		Debug.Log($"Added {classes.Count} classes.");

		InitWeapons();
		InitMagics();
		InitItems();
		InitMaterials();
		InitRecipes();
		InitClasses();
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

		}
	}

	private void InitRecipes()
	{
		if (recipes.Count > 0)
		{
			Alki.Recipe[] curRecipes = Lunatic.Control.MENU.ALKI.Recipes;
			int curSize = curRecipes.Length;
			int newSize = curSize + recipes.Count;
			Alki.Recipe[] newRecipes = new Alki.Recipe[newSize];
			curRecipes.CopyTo(newRecipes, 0);

			for (int i = 0; i < recipes.Count; i++)
				newRecipes[curSize + i] = CreateRecipe(recipes[i]);

			Lunatic.Control.MENU.ALKI.Recipes = newRecipes;
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

	private Alki.Recipe CreateRecipe(ModRecipe recipe)
	{
		Alki alki = Lunatic.Control.MENU.ALKI;

		return new Alki.Recipe
		{
			name = recipe.name,
			des = recipe.description,
			unlocked = recipe.startsUnlocked ? 1 : 0,
			need_1 = System.Array.IndexOf(alki.MATS, recipe.ingredient1.GetName()),
			need_2 = System.Array.IndexOf(alki.MATS, recipe.ingredient2.GetName()),
			need_3 = System.Array.IndexOf(alki.MATS, recipe.ingredient3.GetName()),
			SPAWN = recipe.result
		};
	}

	private Menus.CharClass CreateClass(ModClass @class)
	{
		return new Menus.CharClass
		{
			name = @class.name,
			desc = @class.description,
			LVL = @class.level,
			STR = @class.strength,
			DEF = @class.defense,
			SPD = @class.speed,
			DEX = @class.dexterity,
			INT = @class.intelligence,
			RES = @class.resistance
		};
	}
}
