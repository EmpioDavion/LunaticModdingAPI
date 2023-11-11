// items in the scene that a player can pick up, this is not the actual item,
// it is just a object that gives you an item when you touch it
public class ModItemPickup : Item_Pickup_scr
{
	// the type variable sets the type of item
	// 0 weapon, 1 spell, 2 gold, 3 item, 4 material
	// Alt_Name is a number which indicates its index in the Resources/txt/eng/MATERIALS.txt file,
	// it is only used for alchemy materials

	// TODO: Hook OnPickup, get material by name
	// when the item container is picked up
	public virtual void OnPickup()
	{
		
	}
}
