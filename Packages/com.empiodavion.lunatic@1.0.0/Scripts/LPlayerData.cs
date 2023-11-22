using System.Collections.Generic;

internal class LPlayerData
{
	public class ObjectData
	{
		public virtual string GetDataString(string key)
		{
			return key;
		}
	}

	public class IntegralData : ObjectData
	{
		public int data;

		public override string GetDataString(string key)
		{
			return base.ToString() + data.ToString("00");
		}
	}

	public class MaterialData : IntegralData
	{
		public override string GetDataString(string key)
		{
			int id = Lunatic.GetMaterialID(key);

			return id + data.ToString("00");
		}
	}

	public KeyValuePair<string, IntegralData> weapon1;
	public KeyValuePair<string, IntegralData> weapon2;

	public KeyValuePair<string, ObjectData> magic1;
	public KeyValuePair<string, ObjectData> magic2;

	public Dictionary<string, IntegralData> weapons = new Dictionary<string, IntegralData>();
	public Dictionary<string, ObjectData> magics = new Dictionary<string, ObjectData>();
	public Dictionary<string, IntegralData> items = new Dictionary<string, IntegralData>();
	public Dictionary<string, MaterialData> materials = new Dictionary<string, MaterialData>();
	public Dictionary<string, ObjectData> recipes = new Dictionary<string, ObjectData>();

	public static LPlayerData Save(PlayerData playerData)
	{
		LPlayerData data = Lunatic.GetModData<LPlayerData>() ?? new LPlayerData();

		// weapons
		if (playerData.WEP1.StartsWith("L#"))
		{
			data.weapon1 = GetWeaponData(playerData.WEP1);
			playerData.WEP1 = "";
		}

		if (playerData.WEP2.StartsWith("L#"))
		{
			data.weapon2 = GetWeaponData(playerData.WEP2);
			playerData.WEP2 = "";
		}

		for (int i = 0; i < playerData.WEPS.Length; i++)
		{
			if (string.IsNullOrEmpty(playerData.WEPS[i]))
				break;

			if (!playerData.WEPS[i].StartsWith("L#"))
				continue;

			KeyValuePair<string, IntegralData> weaponData = GetWeaponData(playerData.WEPS[i]);
			data.weapons[weaponData.Key] = weaponData.Value;

			playerData.WEPS[i] = "";
		}

		if (playerData.MAG1.StartsWith("L#"))
		{
			data.magic1 = GetMagicData(playerData.MAG1);
			playerData.MAG1 = "";
		}

		if (playerData.MAG2.StartsWith("L#"))
		{
			data.magic2 = GetMagicData(playerData.MAG2);
			playerData.MAG2 = "";
		}

		for (int i = 0; i < playerData.SPELLS.Length; i++)
		{
			if (string.IsNullOrEmpty(playerData.SPELLS[i]))
				break;

			if (!playerData.SPELLS[i].StartsWith("L#"))
				continue;

			KeyValuePair<string, ObjectData> magicData = GetMagicData(playerData.SPELLS[i]);
			data.magics[magicData.Key] = magicData.Value;

			playerData.SPELLS[i] = "";
		}

		// TODO: save items

		// materials
		for (int i = 0; i < playerData.MATER.Length; i++)
		{
			if (string.IsNullOrEmpty(playerData.MATER[i]))
				break;

			int id = int.Parse(playerData.MATER[i].Substring(0, playerData.MATER[i].Length - 2));

			if (id > Lunatic.BaseMaterialCount * 2)
			{
				playerData.MATER[i] = "";
				ModMaterial material = Lunatic.GetModMaterial(id);

				data.materials[material.InternalName] = new MaterialData
				{
					data = int.Parse(playerData.MATER[i].Substring(playerData.MATER[i].Length - 2))
				};
			}
		}

		// recipes
		foreach (Mod mod in Lunatic.Mods)
		{
			foreach (ModRecipe modRecipe in mod.recipes)
			{
				if (modRecipe.isUnlocked)
				{
					data.recipes[modRecipe.InternalName] = new ObjectData { };
				}
			}
		}

		// send empty strings to end of list
		System.Array.Sort(playerData.WEPS, PushBlankToEnd);
		System.Array.Sort(playerData.SPELLS, PushBlankToEnd);
		System.Array.Sort(playerData.MATER, PushBlankToEnd);

		return data;
	}

	public static void Load(LPlayerData data, PlayerData playerData)
	{
		// weapons
		{
			if (data.weapon1.Value != null)
				playerData.WEP1 = data.weapon1.Value.GetDataString(data.weapon1.Key);

			if (data.weapon2.Value != null)
				playerData.WEP2 = data.weapon2.Value.GetDataString(data.weapon2.Key);

			int index = System.Array.FindIndex(playerData.WEPS, string.IsNullOrEmpty);

			if (index >= 0)
			{
				foreach (KeyValuePair<string, IntegralData> weapon in data.weapons)
				{
					if (index >= playerData.WEPS.Length)
						break;

					playerData.WEPS[index++] = weapon.Value.GetDataString(weapon.Key);
				}
			}
		}

		// magics
		{
			if (data.magic1.Value != null)
				playerData.MAG1 = data.magic1.Value.GetDataString(data.magic1.Key);

			if (data.magic2.Value != null)
				playerData.MAG2 = data.magic2.Value.GetDataString(data.magic2.Key);

			int index = System.Array.FindIndex(playerData.SPELLS, string.IsNullOrEmpty);

			if (index >= 0)
			{
				foreach (KeyValuePair<string, ObjectData> magic in data.magics)
				{
					if (index >= playerData.SPELLS.Length)
						break;

					playerData.SPELLS[index++] = magic.Value.GetDataString(magic.Key);
				}
			}
		}

		// TODO: load items

		// materials
		{
			int index = System.Array.FindIndex(playerData.MATER, string.IsNullOrEmpty);

			if (index >= 0)
			{
				foreach (KeyValuePair<string, MaterialData> material in data.materials)
				{
					if (index >= playerData.MATER.Length)
						break;

					playerData.MATER[index++] = material.Value.GetDataString(material.Key);
				}
			}
		}

		// recipes
		{
			// reset unlocked state
			foreach (Mod mod in Lunatic.Mods)
				foreach (ModRecipe modRecipe in mod.recipes)
					modRecipe.isUnlocked = modRecipe.startsUnlocked;

			foreach (KeyValuePair<string, ObjectData> recipeData in data.recipes)
			{
				Lunatic.ReadInternalName(recipeData.Key, out string modName, out string objectName, false);

				Mod mod = Lunatic.Mods.Find((x) => x.name == modName);

				if (mod != null)
				{
					ModRecipe modRecipe = mod.recipes.Find((x) => x.Name == objectName);

					if (modRecipe != null)
						modRecipe.isUnlocked = true;
				}
			}
		}

		System.Array.Sort(playerData.WEPS, Lunatic.Internal_SortInternalNames);
	}

	private static KeyValuePair<string, IntegralData> GetWeaponData(string weapon)
	{
		int xp = -1;

		if (Lunatic.EndsWithNumbers(weapon, 2))
		{
			xp = int.Parse(weapon.Substring(weapon.Length - 2));
			weapon = weapon.Substring(0, weapon.Length - 2);
		}

		return new KeyValuePair<string, IntegralData>(weapon, new IntegralData { data = xp });
	}

	private static KeyValuePair<string, ObjectData> GetMagicData(string magic)
	{
		return new KeyValuePair<string, ObjectData>(magic, new ObjectData { });
	}

	private static ModWeapon ReadWeaponName(string weapon)
	{
		int slash = weapon.IndexOf('/', 3);
		string modName = weapon.Substring(2, slash - 2);

		Mod mod = Lunatic.Mods.Find((x) => x.name == modName);
		return mod.weapons.Find((x) => x.name == weapon.Substring(0, weapon.Length - 2));
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
}
