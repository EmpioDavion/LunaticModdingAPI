using UnityEditor;

[CustomEditor(typeof(ModItem), true)]
public class ModItemEditor : Editor
{
	protected SerializedProperty itemName;
	protected SerializedProperty sprite;
	protected SerializedProperty type;
	protected SerializedProperty description;
	protected SerializedProperty effectName;
	protected SerializedProperty effect;
	protected SerializedProperty itemCast;

	private void OnEnable()
	{
		itemName = serializedObject.FindProperty("ITEM_NAME");
		sprite = serializedObject.FindProperty("SPR");
		type = serializedObject.FindProperty("type");
		description = serializedObject.FindProperty("desc");
		effectName = serializedObject.FindProperty("effect");
		itemCast = serializedObject.FindProperty("ITM_CAST");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorTools.DrawShowHelpToggle();

		EditorTools.DrawHelpProperty(itemName, "The name of the item.");
		EditorTools.DrawHelpProperty(sprite, "The sprite to use for the item's UI graphic.");

		bool consumable = type.intValue == 0;

		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("If the item gets consumed on use.", MessageType.Info);

		consumable = EditorGUILayout.Toggle(type.displayName, consumable);

		type.intValue = consumable ? 0 : 1;

		EditorTools.DrawHelpProperty(description, "The description of the item to display in the inventory.");
		EditorTools.DrawHelpProperty(effectName, "The name of the particle effect to use.");
		EditorTools.DrawHelpProperty(itemCast, "The path to the resource that will be spawned on use. 0 = nothing.");

		EditorTools.DrawRemainingProperties(serializedObject, itemCast);
	}
}
