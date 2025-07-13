using System.Collections.Generic;
using UnityEngine;

public abstract class LJsonPickup<T> : LJsonAsset<T> where T : Object
{
	public LJsonParamString spawnID;

	public LJsonParamString pickupModel;  // model to use for the pickup

	public LJsonParamJsonTerm displayName;
	public LJsonParamJsonTerm description;

	public LJsonParamAssetSourceArray sources;

	[System.NonSerialized]
	protected ModItemPickup pickupPrefab;

	public override GameObject GetPickup() => pickupPrefab.gameObject;

	protected internal override void SpawnAssets(string currentScene)
	{
		if (sources == null || !ShouldSpawn())
		{
			Debug.Log("Not spawning " + Name);
			return;
		}

		foreach (AssetSource source in sources.value)
			if (source.scene == currentScene)
				SpawnAssetSource(source);
	}

	public void SpawnAssetSource(AssetSource assetSource)
	{
		if (pickupPrefab == null)
		{
			Debug.LogWarning($"Pickup prefab for {Name} is null");
			return;
		}

		switch (assetSource.location)
		{
			case SourceLocation.World:
				{
					Debug.Log($"Spawning {Name} pickup at position {assetSource.position}");

					ModItemPickup clone = Object.Instantiate(pickupPrefab, assetSource.position, Quaternion.identity);

					if (PickupType == Lunatic.ItemTypes.Gold)
						clone.Name = assetSource.count.ToString();

					clone.gameObject.SetActive(true);
				}
				break;
			case SourceLocation.Drop:
				// handled in Loot_scr patch, see Lunatic.Internal_AddLoot
				break;
			case SourceLocation.Shop:
				{
					Debug.Log("Looking for potential shop owners with the name " + assetSource.owner);

					List<GameObject> owners = Lunatic.FindGameObjects(assetSource.owner);

					if (owners.Count == 0)
						Debug.LogWarning("Could not find any object(s) named" + assetSource.owner);
					else
						Debug.Log($"Found {owners.Count} object(s) with the name {assetSource.owner}");

					for (int i = 0; i < owners.Count; i++)
					{
						GameObject owner = owners[i];
						Shop_Inventory shop = owner.GetComponentInChildren<Shop_Inventory>(true);

						if (shop == null)
						{
							Debug.LogWarning($"Could not find shop owned by {assetSource.owner} ({i} of {owners.Count})");
							continue;
						}

						Object prefab = GetPrefab();

						if (prefab == null)
						{
							Debug.LogWarning($"Prefab for {Name} is null");
							return;
						}

						string prefabName = prefab.name;
						string altName = "";

						if (this is LJsonWeapon weapon && weapon.upgrade != null)
							altName = weapon.upgrade.value;

						Shop_Inventory.INV_ITEMS[] newInv = new Shop_Inventory.INV_ITEMS[shop.INV.Length + 1];
						shop.INV.CopyTo(newInv, 0);
						newInv[shop.INV.Length] = new Shop_Inventory.INV_ITEMS
						{
							item = prefabName,
							alt_name = altName,
							OBJ = pickupPrefab.gameObject,
							count = assetSource.count,
							cost = assetSource.value,
							saved_slot = -1,
							type = (int)ShopType,
							Model = null
						};
						shop.INV = newInv;

						Debug.Log($"Added {prefabName} to {assetSource.owner}'s shop.");
					}
				}
				
				break;
		}
	}

	// TODO: allow for purchasing partial amount from shop
	public bool ShouldSpawn()
	{
		if (spawnID != null)
		{
			ModData modData = Lunatic.GetModData<ModData>(Mod);

			if (modData != null)
				return !modData.spawnIDs.TryGetValue(spawnID.value, out int total) || total > 0;
		}

		return true;
	}

	public override T Init(out bool newAsset)
	{
		T equip = base.Init(out newAsset);

		AddLocalisationTerms();
		InitPickup();

		return equip;
	}

	protected void InitPickup()
	{
		pickupPrefab = CreateComponent<ModItemPickup>("BASE ITEM PICKUP");
		pickupPrefab.name = Name + " PICKUP";
		pickupPrefab.item = prefab;
		pickupPrefab.Name = InternalName;
		pickupPrefab.type = (int)PickupType;

		if (pickupModel != null)
		{
			Transform parent = pickupPrefab.transform;
			Vector3 pos = Vector3.zero;

			//if (parent.childCount == 2)
			//{
			//	Transform mdl = parent.GetChild(1);

			//	if (mdl.name == "ITEM_EFF")
			//		mdl = parent.GetChild(0);

			//	pos = mdl.localPosition;
			//	Object.Destroy(mdl.gameObject);
			//}

			GameObject model = LoadModel(pickupModel);

			if (model != null)
			{
				//FixModel(model);

				model = Object.Instantiate(model);
				model.SetActive(true);

				Transform modelTr = model.transform;
				modelTr.SetParent(parent, false);
				modelTr.localPosition = pos;
			}
		}
	}

	protected internal override void AddToLoot(string scene, string owner, List<Loot_scr.Reward> rewards)
	{
		if (sources == null)
			return;

		foreach (AssetSource source in sources.value)
		{
			if (source.location == SourceLocation.Drop && source.owner == owner &&
				string.IsNullOrEmpty(source.scene) || source.scene == scene)
			{
				ModItemPickup pref = pickupPrefab;

				if (PickupType == Lunatic.ItemTypes.Gold)
				{
					pref = Object.Instantiate(pref);
					pref.Name = source.count.ToString();
				}

				rewards.Add(new Loot_scr.Reward
				{
					ITEM = pref.gameObject,
					CHANCE = source.value
				});
			}
		}
	}

	protected void AddLocalisationTerms()
	{
		if (description == null)
			return;

		AddLocalisationTerm(displayName, $"{LanguageFolder}s/{Name}");
		AddLocalisationTerm(description, $"{LanguageFolder} Descriptions/{Name}{LanguageDetailsSuffix}");
	}
}
