using UnityEngine;

public class ModMultipleStates : AREA_SAVED_ITEM
{
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
		foreach (GameObject state in STATES)
			state.SetActive(false);

		STATES[value].SetActive(true);
	}
}
