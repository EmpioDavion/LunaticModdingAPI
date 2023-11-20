using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModItemPickup), true)]
public class ModItemPickupEditor : Editor
{
	private SerializedProperty script;
	private SerializedProperty type;
	private SerializedProperty itemName;
	private SerializedProperty inChest;
	private SerializedProperty saved;

	private string[] materials;

	private void OnEnable()
	{
		script = serializedObject.FindProperty("m_Script");
		type = serializedObject.FindProperty("type");
		itemName = serializedObject.FindProperty("Name");
		inChest = serializedObject.FindProperty("inChest");
		saved = serializedObject.FindProperty("SAVED");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		Lunatic.Init();

		if (materials == null)
			materials = Lunatic.MaterialNames.ToArray();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(script);
		GUI.enabled = true;

		EditorTools.DrawShowHelpToggle();

		EditorTools.DrawItemTypeProperty(type, "Item type contained in this pickup.");

		if (type.intValue == (int)Lunatic.ItemTypes.Material)
		{
			if (EditorTools.ShowHelp)
				EditorGUILayout.HelpBox("You can select one of Lunacid's materials from this list or type your own.", MessageType.Info);

			int index = EditorGUILayout.Popup("Select Lunacid material", -1, materials);

			if (index != -1)
				itemName.stringValue = Lunatic.MaterialNames[index];
		}

		EditorTools.DrawHelpProperty(itemName, "Name of the item or material contained in this pickup.");
		EditorTools.DrawHelpProperty(inChest, "If this item pickup is contained in a chest.");
		EditorTools.DrawHelpProperty(saved, "The object that controls if this item pickup has been collected.");

		EditorTools.DrawRemainingProperties(serializedObject, saved);

		serializedObject.ApplyModifiedProperties();
	}
}
