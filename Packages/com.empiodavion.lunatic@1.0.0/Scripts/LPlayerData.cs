using System.Collections.Generic;

internal class LPlayerData
{
	public class ObjectData
	{
		public string mod;
		public string name;
		public int data;

		public override string ToString()
		{
			return name + data.ToString("00");
		}
	}

	public class MaterialData : ObjectData
	{
		public override string ToString()
		{
			int id = Lunatic.GetMaterialID(name);

			return id + data.ToString("00");
		}
	}

	public ObjectData weapon1;
	public ObjectData weapon2;

	public List<ObjectData> weapons = new List<ObjectData>();
	public List<ObjectData> items = new List<ObjectData>();
	public List<MaterialData> materials = new List<MaterialData>();

	public static LPlayerData Save(PlayerData playerData)
	{
		LPlayerData data = new LPlayerData();

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

			data.weapons.Add(GetWeaponData(playerData.WEPS[i]));
			playerData.WEPS[i] = "";
		}

		for (int i = 0; i < playerData.MATER.Length; i++)
		{
			if (string.IsNullOrEmpty(playerData.MATER[i]))
				break;

			int id = int.Parse(playerData.MATER[i].Substring(0, playerData.MATER[i].Length - 2));

			if (id > Lunatic.BaseMaterialCount * 2)
			{
				playerData.MATER[i] = "";
				ModMaterial material = Lunatic.GetModMaterial(id);

				data.materials.Add(new MaterialData
				{
					mod = material.Mod.name,
					name = material.name,
					data = int.Parse(playerData.MATER[i].Substring(playerData.MATER[i].Length - 2))
				});
			}
		}

		// send empty strings to end of list
		System.Array.Sort(playerData.WEPS, PushBlankToEnd);
		System.Array.Sort(playerData.MATER, PushBlankToEnd);

		return data;
	}

	public static void Load(LPlayerData data, PlayerData playerData)
	{
		int index = System.Array.FindIndex(playerData.MATER, string.IsNullOrEmpty);

		if (index >= 0)
		{
			foreach (MaterialData materialData in data.materials)
			{
				if (index >= playerData.MATER.Length)
					break;

				playerData.MATER[index++] = materialData.ToString();
			}
		}
	}

	private static ObjectData GetWeaponData(string weapon)
	{
		Lunatic.ReadInternalName(weapon, out string modName, out string objectName, true);

		return new MaterialData
		{
			mod = modName,
			name = objectName,
			data = int.Parse(weapon.Substring(weapon.Length - 2))
		};
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
