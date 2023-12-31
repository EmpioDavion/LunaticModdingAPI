﻿// material used for alchemy
// Lunacid seems to only store alchemy materials as strings with appended numbers
public class ModMaterial : UnityEngine.ScriptableObject, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	[System.NonSerialized]
	public int id;

	public string description;

	[UnityEngine.HideInInspector]
	public int count = 1;
}
