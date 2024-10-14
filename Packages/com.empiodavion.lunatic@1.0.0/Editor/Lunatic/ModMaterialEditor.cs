using UnityEditor;

[CustomEditor(typeof(ModMaterial), true)]
public class ModMaterialEditor : ModBaseEditor
{
	protected SerializedProperty description;

	public override SerializedProperty LastProperty => description;

	protected virtual void OnEnable()
	{
		description = serializedObject.FindProperty("description");
	}

	public override void DrawGUI()
	{
		EditorGUILayout.HelpBox(@"Take note that the localisation terms for material descriptions have an underscore _ instead of a space.
Base game Lunacid materials use Materials/MAT## as their terms, where the #s are the material ID.", MessageType.Warning);

		EditorTools.DrawReloadLocalisation();
		EditorTools.DrawTranslation($"Materials/{target.name}", false);
		EditorTools.DrawTranslation($"Material Descriptions/{target.name}_Details", true);

		EditorTools.DrawHelpProperty(description, "The description of the material to display in the inventory.");
	}
}
