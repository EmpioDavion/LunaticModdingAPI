using UnityEngine;

public class LJsonMaterial : LJsonPickup<BaseMaterial>
{
	public override string VanillaFolder => null;
	public override string DefaultAsset => "BASE MATERIAL";
	public override string LanguageFolder => "Material";
	public override string LanguageDetailsSuffix => "_Details";
	public override Lunatic.ShopItemTypes ShopType => Lunatic.ShopItemTypes.None;
	public override Lunatic.ItemTypes PickupType => Lunatic.ItemTypes.Material;
	protected override System.Type SubType => typeof(ModMaterial);

	public override GameObject GetGameObject() => null;
	protected override Object GetPrefab() => null;

	protected override BaseMaterial CreatePrefab<U>(string prefabAsset, System.Type subType = null)
	{
		BaseMaterial result;

		// if MAT#
		// lunacid materials are named MAT1, MAT24, etc (MAT20 is Moon petal)
		if (prefabAsset.Length >= 4 && prefabAsset.StartsWith("MAT") && char.IsDigit(prefabAsset[3]))
		{
			// is lunacid material
			result = (BaseMaterial)prefabAsset;
		}
		else
		{
			// is mod material
			result = (ModMaterial)prefabAsset;
		}

		return result;
	}

	public override BaseMaterial Init(out bool newAsset)
	{
		AddLocalisationTerms();
		InitPickup();

		return base.Init(out newAsset);
	}

	protected override void Set(BaseMaterial obj)
	{
		
	}
}
