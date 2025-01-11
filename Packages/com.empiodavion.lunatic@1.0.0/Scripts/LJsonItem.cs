using UnityEngine;

[System.Serializable]
public class LJsonItem : LJsonPickup<Useable_Item>
{
	public override string VanillaFolder => "ITEMS/";
	public override string DefaultAsset => "BASE ITEM";
	public override string LanguageFolder => "Item";
	public override string LanguageDetailsSuffix => " Details";
	public override Lunatic.ShopItemTypes ShopType => Lunatic.ShopItemTypes.Item;
	public override Lunatic.ItemTypes PickupType => Lunatic.ItemTypes.Item;
	protected override System.Type SubType => typeof(ModItem);

	public LJsonParamString spawnOnUse;	// item to spawn on use
	public LJsonParamString effect;		// name of effect particle (GameObject resource)
	public LJsonParamString sprite;     // sprite image to use

	// need to add effect particle object to AssetReplacement

	public override GameObject GetGameObject() => prefab.gameObject;

	protected override Useable_Item CreatePrefab<U>(string prefabAsset, System.Type subType = null)
	{
		return CreateComponent<Useable_Item>(prefabAsset, subType);
	}

	protected override void Set(Useable_Item obj)
	{
		if (spawnOnUse != null)
		{
			if (obj is ModItem modItem)
			{
				if (FindAsset(spawnOnUse.value, out LJsonAsset toSpawn))
					modItem.spawnOnUse = toSpawn.GetGameObject();
				else
					obj.ITM_CAST = spawnOnUse.value;
			}
			else
				obj.ITM_CAST = spawnOnUse.value;
		}

		SetValue(effect, ref obj.effect);

		obj.SPR = LoadSprite(sprite);
	}
}
