public class ModItem : Useable_Item, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public UnityEngine.GameObject spawnOnUse;

    internal void Init()
    {
        ITEM_NAME = InternalName;
    }

    public virtual void OnUse() { }
}
