public interface IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name { get; }
	public string AssetName { get; set; }
	public string InternalName { get; }
}
