using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Mod.Version))]
public class ModVersionPropertyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty major = property.FindPropertyRelative("major");
		SerializedProperty minor = property.FindPropertyRelative("minor");
		SerializedProperty build = property.FindPropertyRelative("build");
		SerializedProperty revision = property.FindPropertyRelative("revision");

		EditorGUI.LabelField(position, label);

		position.x += EditorGUIUtility.labelWidth;

		position.width = (position.width - EditorGUIUtility.labelWidth) / 4;

		major.intValue = EditorGUI.IntField(position, major.intValue);

		position.x += position.width;

		minor.intValue = EditorGUI.IntField(position, minor.intValue);

		position.x += position.width;

		build.intValue = EditorGUI.IntField(position, build.intValue);

		position.x += position.width;

		revision.intValue = EditorGUI.IntField(position, revision.intValue);
	}
}
