public class ModItem : Useable_Item, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string AssetName { get; set; }

	// TODO: Supply code for GetCastObject

	public virtual void OnUse()
	{

	}

	public virtual UnityEngine.Object GetCastObject()
	{
		return null;
	}
}
