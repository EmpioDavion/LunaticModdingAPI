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
		if (item != null && item is IModObject modObject)
			Name = modObject.InternalName;
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
