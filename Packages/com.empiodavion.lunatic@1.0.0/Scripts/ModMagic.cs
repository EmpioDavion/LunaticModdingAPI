public class ModMagic : Magic_scr, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public ModProjectile projectile;

	public bool InPlayerInventory => Lunatic.PlayerHasMagic(this);

	internal void Init()
	{
		if (projectile != null)
			MAG_CHILD = projectile.InternalName;
	}

	public virtual void OnCast()
	{

	}
}
