// material used for alchemy
// Lunacid seems to only store alchemy materials as strings with appended numbers
public class ModMaterial : BaseMaterial, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	[System.NonSerialized]
	public int id;

	public string description;

	public static explicit operator ModMaterial(string materialName) =>
		Convert<ModMaterial>(materialName);
}
