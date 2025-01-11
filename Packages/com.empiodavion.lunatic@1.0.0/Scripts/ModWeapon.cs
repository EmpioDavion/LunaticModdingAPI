using UnityEngine;

public class ModWeapon : Weapon_scr, IModObject
{
	public Mod Mod { get; set; }
	public AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	[SerializeField]
	private ModWeapon upgradeWeapon;

	public ModSpawnOnEnable projectileSpawner;

	// allows the weapon to use animations and sounds from Lunacid
	// keep as 'None' if the weapon is using its own set(s)
	public LWeapons idleAnimSet = LWeapons.None;
	public LWeapons attackAnimSet = LWeapons.None;
	public LWeapons swingSoundSet = LWeapons.None;
	public LWeapons blockAnimSet = LWeapons.None;
	public LWeapons blockSoundSet = LWeapons.None;

	public bool InPlayerInventory => Lunatic.PlayerHasWeapon(this);

	internal void Init()
	{
		if (idleAnimSet != LWeapons.None)
		{
			Weapon_scr target = GetWeapon(idleAnimSet);
			
			if (target != null)
			{
				Idle = target.Idle;
				Ready_anim = target.Ready_anim;
			}
		}

		AssignSet(attackAnimSet, ref Attack_Anims, (x) => x.Attack_Anims);
		AssignSet(swingSoundSet, ref Swing_snds, (x) => x.Swing_snds);
		AssignSet(blockAnimSet, ref Block_Anims, (x) => x.Block_Anims);
		AssignSet(blockSoundSet, ref Block_snds, (x) => x.Block_snds);

		if (upgradeWeapon != null)
			UPGRADE = upgradeWeapon.InternalName;
	}

	public virtual void OnSwing() { }

	public virtual void OnHit() { }

	public virtual void OnEquip() { }

	public static Weapon_scr GetWeapon(LWeapons weapon)
	{
		string weaponName = weapon.ToString().ToUpper().Replace('_', ' ');
		Weapon_scr target = Resources.Load<Weapon_scr>("WEPS/" + weaponName);

		if (target == null)
			Debug.LogWarning("Could not get weapon set for " + weaponName);

		return target;
	}

	private void AssignSet<T>(LWeapons weapon, ref T[] set, System.Func<Weapon_scr, T[]> getSet)
	{
		Debug.Log(name + " assigning set " + weapon);

		if (weapon == LWeapons.None)
			return;

		Weapon_scr target = GetWeapon(weapon);

		if (target == null)
			return;

		T[] from = getSet(target);
		set = new T[from.Length];
		System.Array.Copy(from, set, from.Length);
	}
}
