using UnityEditor;

[CustomEditor(typeof(ModAutoKill), true)]
public class ModAutoKillEditor : ModBaseEditor
{
	protected SerializedProperty delay;

	public override SerializedProperty LastProperty => delay;

	protected virtual void OnEnable()
	{
		delay = serializedObject.FindProperty("delay");
	}

	public override void DrawGUI()
	{
		EditorTools.DrawHelpProperty(delay, "The number of seconds after spawn to destroy this object.");
	}
}
