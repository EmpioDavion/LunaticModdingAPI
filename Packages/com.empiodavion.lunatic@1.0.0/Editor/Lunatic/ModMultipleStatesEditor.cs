using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModMultipleStates), true)]
public class ModMultipleStatesEditor : Editor
{
	private SerializedProperty script;
	private SerializedProperty id;
	private SerializedProperty value;
	private SerializedProperty states;

	private void OnEnable()
	{
		script = serializedObject.FindProperty("m_Script");
		id = serializedObject.FindProperty("id");
		value = serializedObject.FindProperty("value");
		states = serializedObject.FindProperty("STATES");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(script);
		GUI.enabled = true;

		EditorTools.DrawShowHelpToggle();

		EditorTools.DrawHelpProperty(id, "The id to use for saving and loading the state.");
		EditorTools.DrawHelpProperty(value, "Sets which GameObject in STATES is active. Lunacid objects read this value from a zone string. Mod objects set it directly.");
		EditorTools.DrawHelpProperty(states, "Array of inactive GameObjects that will have one set to active based on the value variable.");

		EditorTools.DrawRemainingProperties(serializedObject, id);

		serializedObject.ApplyModifiedProperties();
	}
}
