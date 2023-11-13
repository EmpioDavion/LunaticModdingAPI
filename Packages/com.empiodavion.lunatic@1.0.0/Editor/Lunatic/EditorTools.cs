using UnityEditor;
using UnityEngine;

public static class EditorTools
{
	public static bool ShowHelp = true;

	private static string[] ItemTypeNames;

	public static void DrawShowHelpToggle()
	{
		ShowHelp = EditorGUILayout.Toggle("Show help", ShowHelp);
	}

	public static void DrawItemTypeProperty(SerializedProperty prop, string help)
	{
		if (ShowHelp)
			EditorGUILayout.HelpBox(help, MessageType.Info);

		if (ItemTypeNames == null)
			ItemTypeNames = typeof(Lunatic.ItemTypes).GetEnumNames();

		prop.intValue = EditorGUILayout.Popup(prop.displayName, prop.intValue, ItemTypeNames);
	}

	public static void DrawHelpProperty(SerializedProperty prop, string help)
	{
		if (ShowHelp)
			EditorGUILayout.HelpBox(help, MessageType.Info);

		EditorGUILayout.PropertyField(prop);
	}

	public static void DrawArrayElement(SerializedProperty arrayProp, int index, string displayName, string help)
	{
		SerializedProperty element = arrayProp.GetArrayElementAtIndex(index);

		if (ShowHelp)
			EditorGUILayout.HelpBox(help, MessageType.Info);

		EditorGUILayout.PropertyField(element, new GUIContent(displayName));
	}

	public static void DrawRemainingProperties(SerializedObject obj, SerializedProperty propsAfter)
	{
		SerializedProperty prop = obj.GetIterator();

		while (prop.NextVisible(true) && prop.name != propsAfter.name)
			;

		while (prop.NextVisible(true))
			EditorGUILayout.PropertyField(prop);
	}
}
