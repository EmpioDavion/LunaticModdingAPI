using System.Collections.Generic;
using UnityEngine;

public enum SourceLocation
{
	World,	// found laying on the ground
	Drop,	// dropped by an enemy
	Shop	// bought at a shop
}

[System.Serializable]
public struct AssetSource
{
	// where the object is found
	public SourceLocation location;

	// the game scene/level that the object is found in
	public string scene;

	// name of enemy that drops the item, or the shop owner, or the scene hierarchy path for the chest
	public string owner;

	// this source will not be used if the save data has this ID
	// can be used to indicate if the item is collected/owned/bought
	public string spawnID;

	// location of the item if found laying on the ground
	public Vector3 position;

	// how much the item costs to buy
	public int value;

	// how many of the item is in the pickup or available to buy
	public int count;

	public override string ToString()
	{
		return @$"Asset Source {{
	Location: {location}
	Scene: {scene}
	Owner: {owner}
	Position: {position}
	Value: {value}
}}";
	}
}

[System.Serializable]
public struct JsonTerm
{
	public string English;
	public string Nyan;
	public string French;
	public string Japanese;
	public string Spanish;
	public string Russian;
	public string German;
	public string Korean;
	public string Polish;
	public string BrazilianPortuguese;
	public string SpanishLATAM;
	public string ChineseTraditional;
	public string ChineseSimplified;
}

public abstract class LJsonAsset : IModObject
{
	public static readonly Dictionary<string, GameObject> jsonModels = new Dictionary<string, GameObject>();

	public static readonly Dictionary<string, Sprite> jsonSprites = new Dictionary<string, Sprite>();

	public Mod Mod { get; set; }
	public AssetBundle Bundle { get; set; }
	public string Name { get; set; }
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public abstract string VanillaFolder { get; }

	public abstract string DefaultAsset { get; }

	public abstract string LanguageFolder { get; }
	public abstract string LanguageDetailsSuffix { get; }

	public abstract Lunatic.ShopItemTypes ShopType { get; }
	public abstract Lunatic.ItemTypes PickupType { get; }

	protected abstract System.Type SubType { get; }

	public abstract GameObject GetGameObject();

	protected abstract Object GetPrefab();

	public virtual GameObject GetPickup() => null;

	protected internal abstract void SpawnAssets(string scene);

	protected internal abstract void AddToLoot(string scene, string owner, List<Loot_scr.Reward> rewards);

	protected Sprite LoadSprite(LJsonParamString str)
	{
		if (str == null)
			return null;

		string path = $"{Mod.folder}/{str.value}";

		if (jsonSprites.TryGetValue(path, out Sprite sprite))
			return sprite;

		if (System.IO.File.Exists(path))
		{
			Texture2D tex = new Texture2D(1, 1);
			byte[] data = System.IO.File.ReadAllBytes(path);
			tex.LoadImage(data);

			sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			jsonSprites.Add(path, sprite);

			return sprite;
		}
		else
			Debug.LogWarning("Could not find sprite at " + path);

		return null;
	}

	protected GameObject LoadModel(LJsonParamString str)
	{
		if (str == null)
			return null;

		//if (!str.value.ToLower().EndsWith(".gltf"))
		//{
		//	// check mod asset bundle for custom model first
		//	GameObject gameObject = Mod.bundle.LoadAsset<GameObject>(str.value);

		//	if (gameObject != null)
		//		return gameObject;

		//	// check for existing asset/resource
		//	gameObject = Resources.Load<GameObject>(str.value);

		//	if (gameObject != null)
		//		return gameObject;
		//}

		string path = $"{Mod.folder}/{str.value}";

		if (jsonModels.TryGetValue(path, out GameObject model))
			return model;

		Debug.Log("Loading model " + path);

		if (!System.IO.File.Exists(path))
		{
			Debug.LogWarning("Could not find mesh file" + path);
			return null;
		}

		GLTF gltf = GLTF.LoadGLTF(path);

		model = gltf.ConvertToGameObject();
		model.SetActive(false);

		Object.DontDestroyOnLoad(model);

		jsonModels.Add(path, model);

		return model;
	}
}

public abstract class LJsonAsset<T> : LJsonAsset where T : Object
{
	[System.NonSerialized]
	protected T prefab;

	protected override Object GetPrefab() => prefab;

	protected virtual U Get<U>(string folder, string asset) where U : Object
	{
		U val = Resources.Load<U>(folder + asset);

		//if (val == null)
		//	val = Resources.Load<U>(folder + Lunatic.CreateInternalName(Mod.name, asset));

		return val;
	}

	protected abstract T CreatePrefab<U>(string prefabAsset, System.Type subType = null);

	protected internal U CreateComponent<U>(string prefabAsset, System.Type subType = null) where U : Component
	{
		GameObject go = Lunatic.MainMod.bundle.LoadAsset<GameObject>(prefabAsset);

		if (go == null)
		{
			Debug.LogWarning($"Could not find prefab {prefabAsset}.");
			return null;
		}

		if (subType == null && go.TryGetComponent<U>(out _))
		{
			go = Object.Instantiate(go);
			Object.DontDestroyOnLoad(go);
			go.SetActive(false);

			return go.GetComponent<U>();
		}
		else if (subType != null && go.TryGetComponent(subType, out _))
		{
			go = Object.Instantiate(go);
			Object.DontDestroyOnLoad(go);
			go.SetActive(false);

			return (U)go.GetComponent(subType);
		}

		Debug.LogWarning($"Could not create prefab for {Name}.");

		return null;
	}

    public virtual T Init(out bool newAsset)
    {
		prefab = Get<T>(VanillaFolder, Name);

		if (prefab == null)
		{
			Debug.Log($"Could not find existing asset {VanillaFolder}{Name}, creating new.");
			prefab = CreatePrefab<T>(DefaultAsset, SubType);
			prefab.name = Name;

			newAsset = true;
		}
		else
			newAsset = false;

        Set(prefab);

		return prefab;
	}

	//private void FixModel(GameObject model)
	//{
	//	if (!model.TryGetComponent(out Weapon_scr weapon))
	//		return;
		
	//	Object.Destroy(weapon);

	//	if (model.TryGetComponent(out AudioSource audio))
	//		Object.Destroy(audio);

	//	Transform tr = weapon.transform;

	//	Transform hand = tr.Find("HAND");

	//	if (hand != null)
	//		Object.Destroy(hand.gameObject);

	//	Transform trails = tr.Find("Trails");

	//	if (trails != null)
	//		Object.Destroy(trails.gameObject);
	//}

	//protected virtual U Create<U>(string folder, string defaultParam) where U : Object
	//{
		//U scr;

		//if (baseParam == null)
		//{
		//	Debug.LogWarning($"Base asset not defined, using {folder}{defaultParam} as base.");
		//	scr = Get<U>(folder, defaultParam);
		//}
		//else
		//{
		//	scr = Get<U>(folder, baseParam);

		//	if (scr == null)
		//	{
		//		Debug.LogWarning($"Could not find base asset: {folder}{baseParam}, using {folder}{defaultParam} as base.");
				//scr = Get<U>(folder, defaultParam);
		//	}
		//}

		//return Object.Instantiate(scr);
	//}

	protected abstract void Set(T obj);

	protected static void SetValue<U>(LJsonParam<U> param, ref U value)
	{
		if (param != null)
			value = param.value;
	}

	protected bool FindAsset(string assetName, out LJsonAsset result)
	{
		result = Mod.jsonAssets.Find((asset) => asset.Name == assetName);

		return result != null;
	}

	protected bool FindAsset<U>(string assetName, out U result) where U : LJsonAsset
	{
		result = Mod.jsonAssets.Find((asset) => asset is U && asset.Name == assetName) as U;

		return result != null;
	}

	protected void AddLocalisationTerm(LJsonParamJsonTerm jsonTerm, string term)
	{
		I2.Loc.TermData termData;

		if (Mod.localisation != null)
			termData = Mod.localisation.mSource.AddTerm(term, I2.Loc.eTermType.Text);
		else
		{
			I2.Loc.LanguageSourceAsset asset = Resources.Load<I2.Loc.LanguageSourceAsset>("I2Languages");
			termData = asset.mSource.AddTerm(term, I2.Loc.eTermType.Text);
		}

		SetTermLanguage(termData, 0, jsonTerm.value.English);
		SetTermLanguage(termData, 1, jsonTerm.value.Nyan);
		SetTermLanguage(termData, 2, jsonTerm.value.French);
		SetTermLanguage(termData, 3, jsonTerm.value.Japanese);
		SetTermLanguage(termData, 4, jsonTerm.value.Spanish);
		SetTermLanguage(termData, 5, jsonTerm.value.Russian);
		SetTermLanguage(termData, 6, jsonTerm.value.ChineseSimplified);
		SetTermLanguage(termData, 7, jsonTerm.value.German);
		SetTermLanguage(termData, 8, jsonTerm.value.Korean);
		SetTermLanguage(termData, 9, jsonTerm.value.Polish);
		SetTermLanguage(termData, 10, jsonTerm.value.BrazilianPortuguese);
		SetTermLanguage(termData, 11, jsonTerm.value.SpanishLATAM);
		SetTermLanguage(termData, 12, jsonTerm.value.ChineseTraditional);
		SetTermLanguage(termData, 13, jsonTerm.value.ChineseSimplified);
	}

	private void SetTermLanguage(I2.Loc.TermData termData, int index, string value)
	{
		if (value != null)
			termData.Languages[index] = value;
	}
}
