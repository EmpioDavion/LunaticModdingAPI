public class ModItem : Useable_Item, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

    // TODO: Supply code for GetCastObject
    internal void Init()
    {
        ITEM_NAME = InternalName;
    }

    public virtual void OnUse()
	{

	}

	public virtual UnityEngine.Object GetCastObject()
	{
		return null;
	}
}
