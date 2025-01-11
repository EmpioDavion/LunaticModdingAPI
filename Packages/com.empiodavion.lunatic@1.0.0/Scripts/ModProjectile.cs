public class ModProjectile : Item_Emit, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	protected ModDamageTrigger damageTrigger;

	protected void Awake()
	{
		damageTrigger = GetComponent<ModDamageTrigger>();
		
		OnSpawn();
	}

	public virtual void OnSpawn() { }
}
