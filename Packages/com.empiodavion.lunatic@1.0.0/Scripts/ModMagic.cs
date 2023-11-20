public class ModMagic : Magic_scr, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string AssetName { get; set; }

	public virtual void OnCast()
	{

	}
}
