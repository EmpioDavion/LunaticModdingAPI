using Newtonsoft.Json;
using System.Collections.Generic;

internal class LPlayerData
{
	public class ObjectData
	{
		public string name;

		public virtual string Load(Mod mod)
		{
			return Lunatic.CreateInternalName(mod.Name, name);
		}
	}

	public class IntegralData : ObjectData
	{
		public int data = -1;

		public override string Load(Mod mod)
		{
			return base.Load(mod) + data.ToString("00");
		}
	}

	public class WeaponData : IntegralData
	{
		public override string Load(Mod mod)
		{
			if (data == -1)
				return Lunatic.CreateInternalName(mod.Name, name);

			return base.Load(mod);
		}
	}

	public class MaterialData : IntegralData
	{
		public override string Load(Mod mod)
		{
			ModMaterial material = mod.materials.Find((x) => x.name == name);

			return material.id + data.ToString("00");
		}
	}

	[System.Serializable]
	public class LPlayerModData
	{
		[Newtonsoft.Json.JsonIgnore]
		public Mod mod;

		public WeaponData weapon1;
		public WeaponData weapon2;

		public ObjectData magic1;
		public ObjectData magic2;

		public IntegralData item1;
		public IntegralData item2;
		public IntegralData item3;
		public IntegralData item4;
		public IntegralData item5;

		public List<WeaponData> weapons = new List<WeaponData>();
		public List<ObjectData> magics = new List<ObjectData>();
		public List<IntegralData> items = new List<IntegralData>();
		public List<MaterialData> materials = new List<MaterialData>();
		public List<ObjectData> recipes = new List<ObjectData>();
	}

	private class LActiveItemMods
	{
		public Mod weapon1;
		public Mod weapon2;

		public Mod magic1;
		public Mod magic2;

		public Mod item1;
		public Mod item2;
		public Mod item3;
		public Mod item4;
		public Mod item5;

		public List<Mod> weapons;
		public List<Mod> magics;
		public List<Mod> items;
		public List<Mod> materials;

		public static LActiveItemMods Create(PlayerData playerData)
		{
			return new LActiveItemMods
			{
				weapon1 = Lunatic.FindModFromInternalName(playerData.WEP1),
				weapon2 = Lunatic.FindModFromInternalName(playerData.WEP2),
				magic1 = Lunatic.FindModFromInternalName(playerData.MAG1),
				magic2 = Lunatic.FindModFromInternalName(playerData.MAG1),
				item1 = Lunatic.FindModFromInternalName(playerData.ITEM1),
				item2 = Lunatic.FindModFromInternalName(playerData.ITEM2),
				item3 = Lunatic.FindModFromInternalName(playerData.ITEM3),
				item4 = Lunatic.FindModFromInternalName(playerData.ITEM4),
				item5 = Lunatic.FindModFromInternalName(playerData.ITEM5),

				weapons = ReadArray(playerData.WEPS),
				magics = ReadArray(playerData.SPELLS),
				items = ReadArray(playerData.ITEMS),
				materials = ReadMaterialArray(playerData.MATER)
			};
		}

		private static List<Mod> ReadArray(string[] array)
		{
			List<Mod> mods = new List<Mod>();

			for (int i = 0; i < array.Length; i++)
			{
				if (string.IsNullOrEmpty(array[i]))
					break;

				mods.Add(Lunatic.FindModFromInternalName(array[i]));
			}

			return mods;
		}

		private static List<Mod> ReadMaterialArray(string[] array)
		{
			Dictionary<int, Mod> materialMods = new Dictionary<int, Mod>();

			foreach (Mod mod in Lunatic.Mods)
				foreach (ModMaterial modMaterial in mod.materials)
					materialMods[modMaterial.id] = mod;

			List<Mod> mods = new List<Mod>();

			for (int i = 0; i < array.Length; i++)
			{
				if (string.IsNullOrEmpty(array[i]))
					break;

				int id = Lunatic.ReadMaterialID(array[i]);

				materialMods.TryGetValue(id, out Mod mod);

				mods.Add(mod);
			}

			return mods;
		}
	}

	[JsonProperty]
	private Dictionary<string, LPlayerModData> playerModDatas = new Dictionary<string, LPlayerModData>();

	private static void CloneArray(ref string[] array)
	{
		string[] newArray = new string[array.Length];
		array.CopyTo(newArray, 0);
		array = newArray;
	}

	public static LPlayerData Delete()
	{
		return new LPlayerData();
	}

	public static LPlayerData Save(PlayerData playerData)
	{
		// create deep copy of playerData arrays
		CloneArray(ref playerData.WEPS);
		CloneArray(ref playerData.SPELLS);
		CloneArray(ref playerData.ITEMS);
		CloneArray(ref playerData.MATER);

		LPlayerData data = Lunatic.GetModData<LPlayerData>(Lunatic.MainMod) ?? new LPlayerData();

		LActiveItemMods itemMods = LActiveItemMods.Create(playerData);

		foreach (Mod mod in Lunatic.Mods)
		{
			LPlayerModData modData = new LPlayerModData() { mod = mod };

			Save(modData, playerData, itemMods);

			data.playerModDatas[mod.Name] = modData;
		}

		// send empty strings to end of list
		System.Array.Sort(playerData.WEPS, PushBlankToEnd);
		System.Array.Sort(playerData.SPELLS, PushBlankToEnd);
		System.Array.Sort(playerData.ITEMS, PushBlankToEnd);
		System.Array.Sort(playerData.MATER, PushBlankToEnd);

		return data;
	}

	private static void Save(LPlayerModData data, PlayerData playerData, LActiveItemMods itemMods)
	{
		// weapons
		if (data.mod == itemMods.weapon1)
		{
			data.weapon1 = GetWeaponData(playerData.WEP1);
			playerData.WEP1 = "";
		}

		if (data.mod == itemMods.weapon2)
		{
			data.weapon2 = GetWeaponData(playerData.WEP2);
			playerData.WEP2 = "";
		}

		for (int i = 0; i < playerData.WEPS.Length; i++)
		{
			if (string.IsNullOrEmpty(playerData.WEPS[i]))
				break;

			if (data.mod != itemMods.weapons[i])
				continue;

			WeaponData weaponData = GetWeaponData(playerData.WEPS[i]);
			data.weapons.Add(weaponData);

			playerData.WEPS[i] = "";
		}

		// magics
		if (data.mod == itemMods.magic1)
		{
			data.magic1 = GetMagicData(playerData.MAG1);
			playerData.MAG1 = "";
		}

		if (data.mod == itemMods.magic2)
		{
			data.magic2 = GetMagicData(playerData.MAG2);
			playerData.MAG2 = "";
		}

		for (int i = 0; i < playerData.SPELLS.Length; i++)
		{
			if (string.IsNullOrEmpty(playerData.SPELLS[i]))
				break;

			if (data.mod != itemMods.magics[i])
				continue;

			ObjectData magicData = GetMagicData(playerData.SPELLS[i]);
			data.magics.Add(magicData);

			playerData.SPELLS[i] = "";
		}

		// items
		if (data.mod == itemMods.item1)
		{
			data.item1 = GetIntegralData(playerData.ITEM1);
			playerData.ITEM1 = "";
		}

		if (data.mod == itemMods.item2)
		{
			data.item2 = GetIntegralData(playerData.ITEM2);
			playerData.ITEM2 = "";
		}

		if (data.mod == itemMods.item3)
		{
			data.item3 = GetIntegralData(playerData.ITEM3);
			playerData.ITEM3 = "";
		}

		if (data.mod == itemMods.item4)
		{
			data.item4 = GetIntegralData(playerData.ITEM4);
			playerData.ITEM4 = "";
		}

		if (data.mod == itemMods.item5)
		{
			data.item5 = GetIntegralData(playerData.ITEM5);
			playerData.ITEM5 = "";
		}

		for (int i = 0; i < playerData.ITEMS.Length; i++)
		{
			if (string.IsNullOrEmpty(playerData.ITEMS[i]))
				break;

			if (data.mod != itemMods.items[i])
				continue;

			IntegralData itemData = GetIntegralData(playerData.ITEMS[i]);
			data.items.Add(itemData);

			playerData.ITEMS[i] = "";
		}

		// materials
		for (int i = 0; i < playerData.MATER.Length; i++)
		{
			if (string.IsNullOrEmpty(playerData.MATER[i]))
				break;

			if (data.mod != itemMods.materials[i])
				continue;

			MaterialData materialData = GetMaterialData(playerData.MATER[i]);
			data.materials.Add(materialData);

			playerData.MATER[i] = "";
		}

		// recipes
		foreach (ModRecipe modRecipe in data.mod.recipes)
		{
			if (modRecipe.isUnlocked)
			{
				ObjectData recipeData = GetRecipeData(modRecipe.Name);
				data.recipes.Add(recipeData);
			}
		}
	}

	public static void Load(LPlayerData data, PlayerData playerData)
	{
		foreach (Mod mod in Lunatic.Mods)
		{
			if (data.playerModDatas.TryGetValue(mod.Name, out var modData))
			{
				modData.mod = mod;
				LoadModData(modData, playerData);
			}
		}

		System.Array.Sort(playerData.WEPS, Lunatic.Internal_SortInternalNames);
		System.Array.Sort(playerData.SPELLS, Lunatic.Internal_SortInternalNames);
		System.Array.Sort(playerData.ITEMS, Lunatic.Internal_SortInternalNames);

		// don't sort materials or recipes
	}

	private static void LoadModData(LPlayerModData data, PlayerData playerData)
	{ 
		// weapons
		{
			if (data.weapon1 != null)
				playerData.WEP1 = data.weapon1.Load(data.mod);

			if (data.weapon2 != null)
				playerData.WEP2 = data.weapon2.Load(data.mod);

			int index = System.Array.FindIndex(playerData.WEPS, string.IsNullOrEmpty);

			if (index >= 0)
			{
				foreach (WeaponData weapon in data.weapons)
				{
					if (index >= playerData.WEPS.Length)
						break;

					playerData.WEPS[index++] = weapon.Load(data.mod);
				}
			}
		}

		// magics
		{
			if (data.magic1 != null)
				playerData.MAG1 = data.magic1.Load(data.mod);

			if (data.magic2 != null)
				playerData.MAG2 = data.magic2.Load(data.mod);

			int index = System.Array.FindIndex(playerData.SPELLS, string.IsNullOrEmpty);

			if (index >= 0)
			{
				foreach (ObjectData magic in data.magics)
				{
					if (index >= playerData.SPELLS.Length)
						break;

					playerData.SPELLS[index++] = magic.Load(data.mod);
				}
			}
		}

		// items
		{
			if (data.item1 != null)
				playerData.ITEM1 = data.item1.Load(data.mod);

			if (data.item2 != null)
				playerData.ITEM2 = data.item2.Load(data.mod);

			if (data.item3 != null)
				playerData.ITEM3 = data.item3.Load(data.mod);

			if (data.item4 != null)
				playerData.ITEM4 = data.item4.Load(data.mod);

			if (data.item5 != null)
				playerData.ITEM5 = data.item5.Load(data.mod);

			int index = System.Array.FindIndex(playerData.ITEMS, string.IsNullOrEmpty);

			if (index >= 0)
			{
				foreach (IntegralData item in data.items)
				{
					if (index >= playerData.ITEMS.Length)
						break;

					playerData.ITEMS[index++] = item.Load(data.mod);
				}
			}
		}

		// materials
		{
			int index = System.Array.FindIndex(playerData.MATER, string.IsNullOrEmpty);

			if (index >= 0)
			{
				foreach (MaterialData material in data.materials)
				{
					if (index >= playerData.MATER.Length)
						break;

					playerData.MATER[index++] = material.Load(data.mod);
				}
			}
		}

		// recipes
		{
			// reset unlocked state
			foreach (Mod mod in Lunatic.Mods)
				foreach (ModRecipe modRecipe in mod.recipes)
					modRecipe.isUnlocked = modRecipe.startsUnlocked;

			foreach (ObjectData recipeData in data.recipes)
			{
				ModRecipe modRecipe = data.mod.recipes.Find((x) => x.Name == recipeData.name);

				if (modRecipe != null)
					modRecipe.isUnlocked = true;
			}
		}
	}

	private static IntegralData GetIntegralData(string internalName)
	{
		int data = -1;

		if (Lunatic.EndsWithNumbers(internalName, 2))
			data = int.Parse(internalName.Substring(internalName.Length - 2));

		string name = Lunatic.ReadObjectName(internalName, false);

		return new IntegralData { name = name, data = data };
	}

	private static WeaponData GetWeaponData(string weapon)
	{
		int data = -1;

		if (Lunatic.EndsWithNumbers(weapon, 2))
			data = int.Parse(weapon.Substring(weapon.Length - 2));

		weapon = Lunatic.ReadObjectName(weapon, false);

		return new WeaponData() { name = weapon, data = data };
	}

	private static ObjectData GetMagicData(string magic)
	{
		magic = Lunatic.ReadObjectName(magic, false);

		return new ObjectData() { name = magic };
	}

	private static MaterialData GetMaterialData(string material)
	{
		int id = Lunatic.ReadMaterialID(material);
		string internalName = Lunatic.GetMaterialName(id);
		string name = Lunatic.ReadObjectName(internalName, false);
		int data = Lunatic.ReadItemData(material);

		return new MaterialData() { name = name, data = data };
	}

	private static ObjectData GetRecipeData(string recipe)
	{
		return new ObjectData() { name = recipe };
	}

	internal static int PushBlankToEnd(string a, string b)
	{
		if (string.IsNullOrEmpty(a))
			return string.IsNullOrEmpty(b) ? 0 : 1;
		else if (string.IsNullOrEmpty(b))
			return -1;

		return 0;
	}
}
