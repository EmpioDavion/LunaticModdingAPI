using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LAssetReference<>), true)]
internal class LAssetReferencePropertyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty reference = property.FindPropertyRelative("reference");

		EditorGUI.PropertyField(position, reference);
	}
}
