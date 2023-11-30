using UnityEditor;

[CustomEditor(typeof(ModObjectLimit), true)]
public class ModObjectLimitEditor : ModBaseEditor
{
	protected SerializedProperty Name;
	protected SerializedProperty max;
	protected SerializedProperty me;

	public override SerializedProperty LastProperty => me;

	private void OnEnable()
	{
		Name = serializedObject.FindProperty("NAME");
		max = serializedObject.FindProperty("MAX");
		me = serializedObject.FindProperty("ME");
	}

	public override void DrawGUI()
	{
		EditorTools.DrawHelpProperty(Name, "The name of the type of object to limit.");
		EditorTools.DrawHelpProperty(max, "The maximum number of instances for that type of object.");
		EditorTools.DrawHelpProperty(me, "If this object should be destroyed instead of the limited object when the limit is surpassed.");
	}
}
