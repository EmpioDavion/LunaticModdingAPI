// this class serves as a container for an item in the world that can be picked up by the player
public class TemplateItemPickup : ModItemPickup
{
	public override void OnPickup()
	{
		TemplateGame.ModData.hubMaterialsCollected = true;
	}
}
