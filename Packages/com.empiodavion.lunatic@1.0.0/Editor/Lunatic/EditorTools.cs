using UnityEditor;
using UnityEngine;

public static class EditorTools
{
	public static bool ShowHelp = true;

	private static string[] ItemTypeNames;

	public static float EditorLineSpacing(int lines)
	{
		return EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing;
	}

	public static float EditorHeight(int lines)
	{
		return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * lines;
	}

	public static void CopyAssetBundleLabel(Object from, Object to)
	{
		string assetPath = AssetDatabase.GetAssetPath(from);
		string bundle = AssetDatabase.GetImplicitAssetBundleName(assetPath);
		string objectPath = AssetDatabase.GetAssetPath(to);
		AssetImporter importer = AssetImporter.GetAtPath(objectPath);
		importer.SetAssetBundleNameAndVariant(bundle, objectPath);
	}

	public static T CreateNewAsset<T>(string path) where T : ScriptableObject
	{
		string assetPath = path;
		int count = 1;

		if (path.EndsWith(".asset"))
			path = path.Substring(0, path.Length - ".asset".Length);
		else
			assetPath += ".asset";

		while (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != null)
		{
			assetPath = path + $" ({count}).asset";
			count++;
		}

		T obj = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset(obj, assetPath);
		AssetDatabase.ImportAsset(assetPath);

		return AssetDatabase.LoadAssetAtPath<T>(assetPath);
		//return obj;
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

	public static void DrawHelpProperty(ref Rect position, SerializedProperty prop, string help)
	{
		if (ShowHelp)
		{
			Rect pos = position;
			pos.height = EditorGUIUtility.singleLineHeight * 2;

			EditorGUI.HelpBox(pos, help, MessageType.Info);

			position.y += EditorLineSpacing(2);
		}

		EditorGUI.PropertyField(position, prop);
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
