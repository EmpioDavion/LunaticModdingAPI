using UnityEngine;

[System.Serializable]
public class LJsonWeapon : LJsonPickup<Weapon_scr>
{
	public override string VanillaFolder => "WEPS/";
	public override string DefaultAsset => "BASE WEAPON";
	public override string LanguageFolder => "Weapon";
	public override string LanguageDetailsSuffix => " details";
	public override Lunatic.ShopItemTypes ShopType => Lunatic.ShopItemTypes.Weapon;
	public override Lunatic.ItemTypes PickupType => Lunatic.ItemTypes.Weapon;
	protected override System.Type SubType => typeof(ModWeapon);

	public LJsonParamString upgrade;
	public LJsonParamFloat attackCooldown;
	public LJsonParamFloat damage;
	public LJsonParamFloat reach;
	public LJsonParamFloat guard;
	public LJsonParamFloat weight;
	public LJsonParamFloat backstep;
	public LJsonParamFloat growth;
	public LJsonParamInt element;
	public LJsonParamBool ranged;
	public LJsonParamString projectileName;
	public LJsonParamVector3 spawnPosition;
	public LJsonParamVector3 spawnRotation;
	public LJsonParamString model;

	public override GameObject GetGameObject() => prefab.gameObject;

	protected override Weapon_scr CreatePrefab<U>(string prefabAsset, System.Type subType = null)
	{
		return CreateComponent<Weapon_scr>(prefabAsset, subType);
	}

	protected override void Set(Weapon_scr obj)
	{
		SetValue(upgrade, ref obj.UPGRADE);
		SetValue(attackCooldown, ref obj.WEP_COOLDOWN);
		SetValue(damage, ref obj.WEP_DAMAGE);
		SetValue(reach, ref obj.WEP_REACH);
		SetValue(guard, ref obj.WEP_GUARD);
		SetValue(weight, ref obj.WEP_WEIGHT);
		SetValue(backstep, ref obj.WEP_BACKSTEP);
		SetValue(growth, ref obj.WEP_GROWTH);
		SetValue(element, ref obj.WEP_ELEMENT);

		obj.type = (ranged?.value ?? false) ? 1 : 0;

		if (upgrade != null && upgrade.value == "")
			obj.WEP_XP = -1.0f;

		if (projectileName != null)
		{
			Transform shoot = obj.transform.Find("SHOOT");
			ModSpawnOnEnable spawnOnEnable = shoot.GetComponent<ModSpawnOnEnable>();

			if (FindAsset(projectileName.value, out LJsonProjectile proj))
				spawnOnEnable.OVERRIDE = proj.GetGameObject();
			else
				spawnOnEnable.item = projectileName.value;

			if (spawnPosition != null)
				shoot.localPosition = spawnPosition.value;

			if (spawnRotation != null)
				shoot.localRotation = Quaternion.Euler(spawnRotation.value);
        }

		GameObject go = LoadModel(model);

		if (go != null)
		{
			Transform mdl = obj.transform.Find("MDL");

			go = Object.Instantiate(go);
			go.transform.SetParent(obj.transform, true);

			Lunatic.RemoveCloneSuffix(go);

			go.SetActive(true);

			if (mdl != null)
				Object.Destroy(mdl.gameObject);
			else
			{
				if (obj.TryGetComponent(out MeshFilter filter))
					Object.Destroy(filter);

				if (obj.TryGetComponent(out MeshRenderer renderer))
					Object.Destroy(renderer);

				go.transform.localPosition = Vector3.zero;
			}
		}
	}
}
