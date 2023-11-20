public class ModWeapon : Weapon_scr, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string AssetName { get; set; }

	public virtual void OnSwing() { }

	public virtual void OnHit() { }

	public virtual void OnEquip() { }
}
