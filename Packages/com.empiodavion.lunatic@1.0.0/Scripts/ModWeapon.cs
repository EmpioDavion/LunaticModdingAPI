public class ModWeapon : Weapon_scr, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	[UnityEngine.SerializeField]
	private ModWeapon upgradeWeapon;

	public bool InPlayerInventory => Lunatic.PlayerHasWeapon(this);

	internal void Init()
	{
		if (upgradeWeapon != null)
			UPGRADE = upgradeWeapon.InternalName;
	}

	public virtual void OnSwing() { }

	public virtual void OnHit() { }

	public virtual void OnEquip() { }
}
