using UnityEditor;

[CustomEditor(typeof(ModMultipleStates), true)]
public class ModMultipleStatesEditor : ModBaseEditor
{
	private SerializedProperty id;
	private SerializedProperty value;
	private SerializedProperty states;

	public override SerializedProperty LastProperty => id;

	private void OnEnable()
	{
		id = serializedObject.FindProperty("id");
		value = serializedObject.FindProperty("value");
		states = serializedObject.FindProperty("STATES");
	}

	public override void DrawGUI()
	{
		EditorTools.DrawReloadLocalisation();
		
		string trans = id.stringValue;

		if (string.IsNullOrEmpty(trans))
		{
			ModMultipleStates ms = (ModMultipleStates)target;
			ModDialog md = ms.GetComponentInChildren<ModDialog>(true);

			if (md != null)
				trans = md.npc_name;
		}

		EditorTools.DrawTranslation($"Dialog/{trans} NAME", false);

		EditorTools.DrawHelpProperty(id, "The id to use for saving and loading the state.");
		EditorTools.DrawHelpProperty(value, "Sets which GameObject in STATES is active. Lunacid objects read this value from a zone string. Mod objects set it directly.");
		EditorTools.DrawHelpProperty(states, "Array of inactive GameObjects that will have one set to active based on the value variable.");
	}
}
