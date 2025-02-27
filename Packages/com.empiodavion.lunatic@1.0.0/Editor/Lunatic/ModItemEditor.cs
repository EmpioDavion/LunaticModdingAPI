using UnityEditor;

[CustomEditor(typeof(ModItem), true)]
public class ModItemEditor : ModBaseEditor
{
	protected SerializedProperty itemName;
	protected SerializedProperty sprite;
	protected SerializedProperty type;
	protected SerializedProperty description;
	protected SerializedProperty effectName;
	protected SerializedProperty effect;
	protected SerializedProperty itemCast;
	protected SerializedProperty spawnOnUse;

	public override SerializedProperty LastProperty => spawnOnUse;

	private void OnEnable()
	{
		itemName = serializedObject.FindProperty("ITEM_NAME");
		sprite = serializedObject.FindProperty("SPR");
		type = serializedObject.FindProperty("type");
		description = serializedObject.FindProperty("desc");
		effectName = serializedObject.FindProperty("effect");
		itemCast = serializedObject.FindProperty("ITM_CAST");
		spawnOnUse = serializedObject.FindProperty("spawnOnUse");
	}

	public override void DrawGUI()
	{
		EditorTools.DrawReloadLocalisation();
		EditorTools.DrawTranslation($"Items/{target.name}", false);
		EditorTools.DrawTranslation($"Item Descriptions/{target.name} details", true);

		EditorTools.DrawHelpProperty(itemName, "The name of the item.");
		EditorTools.DrawHelpProperty(sprite, "The sprite to use for the item's UI graphic.");

		bool consumable = type.intValue == 0;

		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("If the item gets consumed on use.", MessageType.Info);

		consumable = EditorGUILayout.Toggle("Consumable", consumable);

		type.intValue = consumable ? 0 : 1;

		EditorTools.DrawHelpProperty(description, "The description of the item to display in the inventory.");
		EditorTools.DrawHelpProperty(effectName, "The name of the particle effect to use.");

		UnityEngine.GUI.enabled = spawnOnUse.objectReferenceValue == null;

		EditorTools.DrawHelpProperty(itemCast, "The path to the resource that will be spawned on use. 0 = nothing.");

		UnityEngine.GUI.enabled = true;

		EditorTools.DrawHelpProperty(spawnOnUse, "The object to spawn when using the item. Overrides ITM_CAST");
	}
}
