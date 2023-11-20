public class ModMultipleStates : AREA_SAVED_ITEM, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public virtual void OnLoad()
	{
		
	}

	public virtual void OnSave()
	{

	}

	public virtual void OnTalk(Dialog dialog)
	{

	}

	public void ActivateStage()
	{
		foreach (UnityEngine.GameObject state in STATES)
			state.SetActive(false);

		STATES[value].SetActive(true);
	}
}
