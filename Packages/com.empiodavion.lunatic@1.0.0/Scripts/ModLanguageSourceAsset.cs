using UnityEngine;

[CreateAssetMenu(menuName = "Lunatic/Language Source Asset")]
public class ModLanguageSourceAsset : I2.Loc.LanguageSourceAsset, IModObject
{
	public Mod Mod { get; set; }
	public AssetBundle Bundle { get; set; }

	public string Name => name;

	public string AssetName { get; set; }

	public string InternalName => Lunatic.GetInternalName(this);

	private void Awake()
	{
#if UNITY_EDITOR
		if (mSource.mLanguages.Count == 0)
		{
			I2.Loc.LanguageSourceAsset asset = Resources.Load<I2.Loc.LanguageSourceAsset>("I2Languages");
			mSource.mLanguages = asset.mSource.mLanguages;
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif

		mSource.owner = this;
	}

	public virtual void OnTryGetTranslation(string term, ref string Translation,
		string overrideLanguage = null, string overrideSpecialization = null,
		bool skipDisabled = false, bool allowCategoryMistmatch = false)
	{

	}
}
