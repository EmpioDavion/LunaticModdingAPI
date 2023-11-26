// items in the scene that a player can pick up, this is not the actual item,
// it is just a object that gives you an item when you touch it
public class ModItemPickup : Item_Pickup_scr, IModObject
{
	// the type variable sets the type of item
	// 0 weapon, 1 magic, 2 gold, 3 item, 4 material
	// Alt_Name is a number which indicates its index in the Resources/txt/eng/MATERIALS.txt file,
	// it is only used for alchemy materials

	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	string IModObject.Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public UnityEngine.Object item;

	internal void Init()
	{
		if (Name.StartsWith("L#"))
		{
			IModObject modObject = null;

			switch ((Lunatic.ItemTypes)type)
			{
				case Lunatic.ItemTypes.Weapon:
					modObject = Mod.weapons.Find(MatchName);
					break;
				case Lunatic.ItemTypes.Magic:
					modObject = Mod.magics.Find(MatchName);
					break;
				case Lunatic.ItemTypes.Item:
					modObject = Mod.items.Find(MatchName);
					break;
				case Lunatic.ItemTypes.Material:
					modObject = Mod.materials.Find(MatchName);
					break;
				default:
					break;
			}

			if (modObject == null)
				UnityEngine.Debug.LogError($"Could not find {Name} for item pickup {name}");
		}
	}

	public bool Internal_CheckStart()
	{
		if (type == (int)Lunatic.ItemTypes.Material)
		{
			Alt_Name = Lunatic.GetMaterialID(Name).ToString();
			return true;
		}

		return false;
	}

	private bool MatchName<T>(T modObject) where T : IModObject
	{
		return modObject.InternalName == Name;
	}

	public virtual void OnPickup()
	{
		
	}

	public ModItemPickup Spawn(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
	{
		return Instantiate(this, position, rotation);
	}

	public ModItemPickup SpawnOnPlayer()
	{
		UnityEngine.Transform playerTransform = Lunatic.Player.transform;

		return Spawn(playerTransform.position, playerTransform.rotation);
	}
}
