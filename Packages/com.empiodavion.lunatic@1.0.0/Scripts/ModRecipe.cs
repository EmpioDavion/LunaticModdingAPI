using UnityEngine;

public class ModRecipe : ScriptableObject
{
	[System.Serializable]
	public struct Ingredient
	{
		public enum Mode
		{
			Reference,
			String
		}

		public Mode mode;
		public string name;
		public ModMaterial material;

		public string GetName()
		{
			return mode == Mode.Reference ? material.name : name;
		}
	}

	public string description;

	public bool startsUnlocked;

	public Ingredient ingredient1;
	public Ingredient ingredient2;
	public Ingredient ingredient3;

	public GameObject result;
}
