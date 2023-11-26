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

public class ModRecipe : ScriptableObject, IModObject, ISerializationCallbackReceiver
{
	public Mod Mod { get; set; }
	public AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public string description;

	public bool startsUnlocked;

	public bool isUnlocked;

	public ModMaterial material1;
	public ModMaterial material2;
	public ModMaterial material3;

	[SerializeField, HideInInspector]
	internal string ingredient1Name;

	[SerializeField, HideInInspector]
	internal string ingredient2Name;

	[SerializeField, HideInInspector]
	internal string ingredient3Name;

	[System.NonSerialized]
	public Ingredient ingredient1;

	[System.NonSerialized]
	public Ingredient ingredient2;

	[System.NonSerialized]
	public Ingredient ingredient3;

	public GameObject result;

	[System.NonSerialized]
	public int recipeIndex = -1;

	internal void Init()
	{
		isUnlocked |= startsUnlocked;

		ingredient1.Init(this, ingredient1Name);
		ingredient2.Init(this, ingredient2Name);
		ingredient3.Init(this, ingredient3Name);
	}

	protected internal virtual void OnUnlocked(Alki.Recipe recipe)
	{

	}

	protected internal virtual void OnForged(Alki.Recipe recipe)
	{

	}

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
		static void GetMaterialName(ModMaterial material, ref string ingredientName)
		{
			if (material == null)
				return;

			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(material);

			if (assetPath.StartsWith("Assets/"))
			{
				Mod mod = UnityEditor.AssetDatabase.LoadAssetAtPath<Mod>("Assets/Mod.asset");
				ingredientName = Lunatic.CreateInternalName(mod.Name, material.Name);
			}
			else
				ingredientName = material.name;
		}

		GetMaterialName(material1, ref ingredient1Name);
		GetMaterialName(material2, ref ingredient2Name);
		GetMaterialName(material3, ref ingredient3Name);
#endif
	}

	public void OnAfterDeserialize()
	{

	}
}
