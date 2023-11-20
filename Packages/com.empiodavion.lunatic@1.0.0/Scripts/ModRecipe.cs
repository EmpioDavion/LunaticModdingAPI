using UnityEngine;

// for some reason this struct is unable to be loaded from asset bundles
[System.Serializable]
public struct Ingredient
{
	public string name;

	[System.NonSerialized]
	public ModMaterial material;

	internal void Init(ModRecipe recipe, string _name)
	{
		name = _name;
		material = recipe.Mod.materials.Find((x) => x.name == _name);
	}

	internal int GetID()
	{
		return material == null ? Lunatic.GetMaterialID(name) : material.id;
	}
}

public class ModRecipe : ScriptableObject, IModObject
{
	public Mod Mod { get; set; }
	public AssetBundle Bundle { get; set; }
	public string AssetName { get; set; }

	public string description;

	public bool startsUnlocked;

	[SerializeField]
	private string ingredient1Name;

	[SerializeField]
	private string ingredient2Name;

	[SerializeField]
	private string ingredient3Name;

	[System.NonSerialized]
	public Ingredient ingredient1;

	[System.NonSerialized]
	public Ingredient ingredient2;

	[System.NonSerialized]
	public Ingredient ingredient3;

	public GameObject result;

	internal void Init()
	{
		ingredient1.Init(this, ingredient1Name);
		ingredient2.Init(this, ingredient2Name);
		ingredient3.Init(this, ingredient3Name);
	}
}
