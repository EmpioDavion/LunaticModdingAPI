using UnityEngine;

[System.Serializable]
public class LJsonMagic : LJsonPickup<Magic_scr>
{
	public override string VanillaFolder => "MAGIC/";
	public override string DefaultAsset => "BASE MAGIC";
	public override string LanguageFolder => "Spell";
	public override string LanguageDetailsSuffix => " details";
	public override Lunatic.ShopItemTypes ShopType => Lunatic.ShopItemTypes.Ring;
	public override Lunatic.ItemTypes PickupType => Lunatic.ItemTypes.Magic;
	protected override System.Type SubType => typeof(ModMagic);

	public LJsonParamString icon;
	public LJsonParamColour colour;
	public LJsonParamInt type;
	public LJsonParamFloat damage;
	public LJsonParamInt element;
	public LJsonParamFloat maxChargeTime;
	public LJsonParamFloat minChargeTime;
	public LJsonParamString projectileName;
	public LJsonParamFloat projectileLifetime;
	public LJsonParamFloat cost;
	public LJsonParamFloat flashFade;
	public LJsonParamBool costsHealth;

	public override GameObject GetGameObject() => prefab.gameObject;

	protected override Magic_scr CreatePrefab<U>(string prefabAsset, System.Type subType = null)
	{
		return CreateComponent<Magic_scr>(prefabAsset, subType);
	}

	protected override void Set(Magic_scr obj)
	{
		SetValue(colour, ref obj.MAG_COLOR);
		SetValue(type, ref obj.MAG_TYPE);
		SetValue(damage, ref obj.MAG_DAMAGE);
		SetValue(element, ref obj.MAG_ELEM);
		SetValue(maxChargeTime, ref obj.MAG_CHARGE_TIME);
		SetValue(minChargeTime, ref obj.MIN_CHARGE_TIME);

		if (projectileName != null)
		{
			if (obj is ModMagic modMagic)
			{
				if (FindAsset(projectileName.value, out LJsonProjectile projectile))
					modMagic.projectile = projectile.GetGameObject().GetComponent<ModProjectile>();
				else
					obj.MAG_CHILD = projectileName.value;
			}
			else
				obj.MAG_CHILD = projectileName.value;
		}

		SetValue(projectileLifetime, ref obj.MAG_LIFE);
		SetValue(cost, ref obj.MAG_COST);
		SetValue(flashFade, ref obj.MAG_FADE);
		SetValue(costsHealth, ref obj.MAG_BL);

		obj.ICON = LoadSprite(icon);

		obj.gameObject.SetActive(true);
	}
}
