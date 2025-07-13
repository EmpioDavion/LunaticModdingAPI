using System.Collections.Generic;
using UnityEngine;

public class LJsonProjectile : LJsonAsset<Item_Emit>
{
	public override string VanillaFolder => "MAGIC/CAST/";
	public override string DefaultAsset => "BASE PROJECTILE";
	public override string LanguageFolder => "";
	public override string LanguageDetailsSuffix => "";
	public override Lunatic.ShopItemTypes ShopType => Lunatic.ShopItemTypes.None;
	public override Lunatic.ItemTypes PickupType => Lunatic.ItemTypes.None;
	protected override System.Type SubType => typeof(ModProjectile);

	public LJsonParamFloat speedIncrease;
	public LJsonParamFloat initialForce;

	public LJsonParamFloat damage;
	public LJsonParamInt element;
	public LJsonParamFloat shake;
	public LJsonParamFloat shakeLength;
	public LJsonParamString impactEffect;
	public LJsonParamBool physical;
	public LJsonParamBool affectsPlayer;
	public LJsonParamBool affectsNonPlayer;
	public LJsonParamBool constantDamage;
	public LJsonParamFloat tickDelay;
	public LJsonParamString spawnOnHit;
	public LJsonParamBool spawnOnConstant;
	public LJsonParamBool stickToHit;

	public override GameObject GetGameObject() => prefab.gameObject;

	protected override Item_Emit CreatePrefab<U>(string prefabAsset, System.Type subType = null)
	{
		return CreateComponent<Item_Emit>(prefabAsset, subType);
	}

	protected internal override void SpawnAssets(string scene) { }

	protected internal override void AddToLoot(string scene, string owner, List<Loot_scr.Reward> rewards) { }

	protected override void Set(Item_Emit obj)
	{
		// item emit
		SetValue(speedIncrease, ref obj.speed_increase);
		SetValue(initialForce, ref obj.force);

		// damage trigger
		if (!obj.TryGetComponent(out Damage_Trigger dmg))
		{
			if (!obj.TryGetComponent(out ModDamageTrigger mdmg))
				return;
			else
				dmg = mdmg;
		}

		SetValue(damage, ref dmg.power);
		SetValue(element, ref dmg.element);
		SetValue(shake, ref dmg.shake);
		SetValue(shakeLength, ref dmg.shake_length);
		
		if (dmg.IMP = !string.IsNullOrEmpty(impactEffect?.value))
			dmg.tag = impactEffect.value;

		SetValue(physical, ref dmg.physical);
		SetValue(affectsPlayer, ref dmg.EffectPlayer);
		
		// value needs to be inverted
		if (affectsNonPlayer != null)
			dmg.OnlyPL = !affectsNonPlayer.value;

		SetValue(constantDamage, ref dmg.Constant);
		SetValue(tickDelay, ref dmg.frequency);

		if (dmg is ModDamageTrigger modDmg)
		{
			if (!string.IsNullOrEmpty(spawnOnHit?.value))
			{
				dmg.Child_Effect = false;
				SetValue(spawnOnHit, ref modDmg.spawnOnHit);
			}

			SetValue(spawnOnConstant, ref modDmg.spawnOnConstant);
		}

		SetValue(stickToHit, ref dmg.arrow);
	}
}
