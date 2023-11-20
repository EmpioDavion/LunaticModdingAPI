// this class serves as a container for an item in the world that can be picked up by the player
public class TemplateItemPickup : ModItemPickup
{
	private void Awake()
	{
		if (type == (int)Lunatic.ItemTypes.Material)
			Alt_Name = name;
	}

	public override void OnPickup()
	{
		
	}
}
