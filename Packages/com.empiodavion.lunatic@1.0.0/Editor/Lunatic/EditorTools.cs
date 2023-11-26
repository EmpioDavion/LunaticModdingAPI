using UnityEditor;
using UnityEngine;

public static class EditorTools
{
	public static bool ShowHelp = true;

	private static string[] ItemTypeNames;

	public static T CreateNewAsset<T>(string path) where T : ScriptableObject
	{
		string assetPath = path;
		int count = 1;

		if (path.EndsWith(".asset"))
			path = path.Substring(0, path.Length - ".asset".Length);

		while (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != null)
		{
			assetPath = path + $" ({count})";
			count++;
		}

		assetPath += ".asset";

		T obj = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset(obj, assetPath);
		AssetDatabase.ImportAsset(assetPath);

		return obj;
	}

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

	public static void CheckLunaticRef(SerializedProperty prop, SerializedProperty nameProp)
	{
		if (prop.objectReferenceValue == null)
			nameProp.stringValue = "";
		else
		{
			string propName = prop.objectReferenceValue.name;
			GetLunaticName(prop.objectReferenceValue, ref propName);

			nameProp.stringValue = propName;
		}
	}

	public static void GetLunaticName(Object modObject, ref string name)
	{
		if (modObject == null)
			return;

		string assetPath = AssetDatabase.GetAssetPath(modObject);

		if (assetPath.StartsWith("Assets/"))
		{
			Mod mod = AssetDatabase.LoadAssetAtPath<Mod>("Assets/Mod.asset");
			name = Lunatic.CreateInternalName(mod.Name, modObject.name);
		}
		else
			name = modObject.name;
	}

	public static void DrawRefName(SerializedProperty prop, SerializedProperty nameProp)
	{
		GUI.enabled = false;

		if (prop.objectReferenceValue != null)
			EditorGUILayout.TextField("Object Name", prop.objectReferenceValue.name);
		else
			EditorGUILayout.TextField("Object Name", nameProp.stringValue);

		GUI.enabled = true;
	}
}
