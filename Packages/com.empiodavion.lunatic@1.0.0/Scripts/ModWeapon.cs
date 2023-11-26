public class ModWeapon : Weapon_scr, IModObject, UnityEngine.ISerializationCallbackReceiver
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	[UnityEngine.SerializeField]
	private ModWeapon upgradeWeapon;

	internal void Init()
	{
		if (UPGRADE.StartsWith("L#"))
		{
			ModWeapon modWeapon = Mod.weapons.Find(MatchName);

			if (modWeapon == null)
				UnityEngine.Debug.LogError($"Could not find {Name} for item pickup {name}");
		}
	}

	public virtual void OnSwing() { }

	public virtual void OnHit() { }

	public virtual void OnEquip() { }

	private bool MatchName<T>(T modObject) where T : IModObject
	{
		return modObject.InternalName == Name;
	}

	public void OnBeforeSerialize()
	{
#if UNITY_EDITOR
		if (upgradeWeapon != null)
		{
			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(upgradeWeapon);

			if (assetPath.StartsWith("Assets/"))
				UPGRADE = assetPath;
		}
#endif
	}

	public void OnAfterDeserialize()
	{
		
	}
}
