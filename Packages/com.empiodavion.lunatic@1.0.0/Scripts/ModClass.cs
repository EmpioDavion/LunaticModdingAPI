public class ModClass : UnityEngine.ScriptableObject, IModObject
{
	[System.Serializable]
	public struct DamageMultipliers
	{
		public float normal;
		public float fire;
		public float ice;
		public float poison;
		public float light;
		public float dark;

		// TODO: allow for new damage types
	}

	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public string description;

	public int level;
	public int strength;
	public int defense;
	public int speed;
	public int dexterity;
	public int intelligence;
	public int resistance;

	public DamageMultipliers damageMultipliers;

	internal void SetDamageMultipliers(CONTROL control)
	{
		control.NORMAL_MULT = damageMultipliers.normal;
		control.FIRE_MULT = damageMultipliers.fire;
		control.ICE_MULT = damageMultipliers.ice;
		control.POISON_MULT = damageMultipliers.poison;
		control.LIGHT_MULT = damageMultipliers.light;
		control.DARK_MULT = damageMultipliers.dark;
	}
}
