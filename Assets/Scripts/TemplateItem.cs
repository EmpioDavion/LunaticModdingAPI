using UnityEngine;

public class TemplateItem : ModItem
{
	public override void OnUse()
	{
		
	}

	public override Object GetCastObject()
	{
		// you can load Lunacid game assets that are found under Lunacid/Resources/
		return Resources.Load("items/HEALTH_VIAL_PICKUP");

		// you can load from your asset bundle
		//return TemplateGame.MyAssets.LoadAsset("TestItemCast");
	}
}
