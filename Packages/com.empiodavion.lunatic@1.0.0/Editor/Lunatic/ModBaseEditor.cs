using UnityEditor;
using UnityEngine;

public abstract class ModBaseEditor : Editor
{
	public abstract SerializedProperty LastProperty { get; }

	public bool drawHeader = true;

	public override void OnInspectorGUI()
	{
		if (drawHeader)
		{
			SerializedProperty script = serializedObject.FindProperty("m_Script");

			GUI.enabled = false;
			EditorGUILayout.PropertyField(script);
			GUI.enabled = true;

			EditorTools.DrawShowHelpToggle();
		}

		serializedObject.Update();

		DrawGUI();

		if (LastProperty != null)
			EditorTools.DrawRemainingProperties(serializedObject, LastProperty);

		serializedObject.ApplyModifiedProperties();
	}

	public abstract void DrawGUI();
}
