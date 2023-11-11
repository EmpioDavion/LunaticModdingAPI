using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModMultipleStates), true)]
public class ModMultipleStatesEditor : Editor
{
	private SerializedProperty script;
	private SerializedProperty value;
	private SerializedProperty states;
	private SerializedProperty currentString;

	private void OnEnable()
	{
		script = serializedObject.FindProperty("m_Script");
		value = serializedObject.FindProperty("value");
		states = serializedObject.FindProperty("STATES");
		currentString = serializedObject.FindProperty("current_string");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(script);
		GUI.enabled = true;

		EditorTools.DrawShowHelpToggle();

		EditorTools.DrawHelpProperty(value, "Sets which GameObject in STATES is active. Lunacid objects read this value from a zone string. Mod objects set it directly.");
		EditorTools.DrawHelpProperty(states, "Array of inactive GameObjects that will have one set to active based on the value variable.");

		EditorTools.DrawRemainingProperties(serializedObject, currentString);

		serializedObject.ApplyModifiedProperties();
	}
}
