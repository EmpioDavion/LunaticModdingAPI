using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LReference<>), true)]
internal class LReferencePropertyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight * 2;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty reference = property.FindPropertyRelative("reference");

		EditorGUI.PropertyField(position, reference);
	}
}
