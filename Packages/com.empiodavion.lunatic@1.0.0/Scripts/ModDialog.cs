public class ModDialog : Dialog, IModObject
{
	public enum DialogResponses
	{
		Exit,
		AutoExit,
		YesNo,
		None,
		Trigger,
		Shop
	}

	public Mod Mod { get; set; }
	public UnityEngine.AssetBundle Bundle { get; set; }
	public string Name => name;
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	public string id;

	private float dummyBlendShape = 0.0f;

	public void Internal_Init()
	{
		if (CON == null)
			CON = Lunatic.Control;

		//for (int i = 0; i < LINES.Length; i++)
		//	LINES[i].value = LINES[i].value.Replace("/c", CON.CURRENT_PL_DATA.PLAYER_NAME);

		OBJS[0] = Lunatic.UIReferences.PlayerTypedText;
		OBJS[1] = Lunatic.UIReferences.PlayerName;

		OBJS2[0] = Lunatic.UIReferences.PlayerResponseYes;
		OBJS2[1] = Lunatic.UIReferences.PlayerResponseNo;
		OBJS2[2] = Lunatic.UIReferences.PlayerResponseExit;
	}

	public void Internal_OnSpeak(Line line)
	{
		OnSpeak(line);
	}

	protected virtual void OnSpeak(Line line)
	{

	}

	public static float GetBlendShapeWeight(UnityEngine.SkinnedMeshRenderer renderer, int index, Dialog dialog)
	{
		if (dialog is ModDialog modDialog)
		{
			if (renderer != null && renderer.sharedMesh != null && index < renderer.sharedMesh.blendShapeCount)
				return renderer.GetBlendShapeWeight(index);

			if (index == 0 && renderer != null && renderer == modDialog.Mouth)
				return modDialog.dummyBlendShape;

			return 0.0f;
		}

		return renderer.GetBlendShapeWeight(index);
	}

	public static void SetBlendShapeWeight(UnityEngine.SkinnedMeshRenderer renderer, int index, float weight, Dialog dialog)
	{
		if (dialog is ModDialog modDialog)
		{
			if (renderer != null && renderer.sharedMesh != null && index < renderer.sharedMesh.blendShapeCount)
				renderer.SetBlendShapeWeight(index, weight);

			if (index == 0 && renderer != null && renderer == dialog.Mouth)
				modDialog.dummyBlendShape = weight;
		}
		else
			renderer.SetBlendShapeWeight(index, weight);
	}
}
