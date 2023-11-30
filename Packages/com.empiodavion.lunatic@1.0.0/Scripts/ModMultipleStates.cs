public class ModMultipleStates : AREA_SAVED_ITEM, IModObject
{
	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public string id;

	private void Awake()
	{
		CON = Lunatic.Control;
	}

	public void Internal_Load()
	{
		if (CON == null)
			CON = Lunatic.Control;

		Mod.npcStates.TryGetValue(id, out value);

		for (int i = 0; i < STATES.Length; i++)
			STATES[i].SetActive(false);
		
		STATES[value].SetActive(true);

		OnLoad();
	}

	public void Internal_Save()
	{
		if (CON == null)
			CON = Lunatic.Control;

		if (!string.IsNullOrEmpty(id))
			Mod.npcStates[id] = value;

		OnSave();
	}

	protected virtual void OnLoad()
	{
		
	}

	protected virtual void OnSave()
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
