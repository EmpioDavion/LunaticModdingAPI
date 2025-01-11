using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorTools
{
	private const string SHOW_HELP_NAME = "Lunatic Show Help";
	private const string LOCALISATION_ASSET = "Assets/Localisation.asset";

	private static bool _ShowHelp = true;
	public static bool ShowHelp
	{ 
		get => _ShowHelp; 
		set => EditorPrefs.SetBool(SHOW_HELP_NAME, _ShowHelp = value);
	}

	private static string[] ItemTypeNames;

	static EditorTools()
	{
		_ShowHelp = EditorPrefs.GetBool(SHOW_HELP_NAME, true);
	}

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
		importer.SetAssetBundleNameAndVariant(bundle, "");
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
	}

	public static void DrawShowHelpToggle()
	{
		bool value = EditorGUILayout.Toggle("Show help", _ShowHelp);

		if (value != _ShowHelp)
			ShowHelp = value;
	}

	public static void DrawHelpBox(ref Rect position, string help)
	{
		if (ShowHelp)
		{
			Rect pos = position;
			pos.height = EditorGUIUtility.singleLineHeight * 2;

			EditorGUI.HelpBox(pos, help, MessageType.Info);

			position.y += EditorLineSpacing(2);
		}
	}

	public static void DrawHelpBox(string help)
	{
		if (ShowHelp)
			EditorGUILayout.HelpBox(help, MessageType.Info);
	}

	public static void DrawItemTypeProperty(SerializedProperty prop, string help)
	{
		DrawHelpBox(help);

		if (ItemTypeNames == null)
			ItemTypeNames = typeof(Lunatic.ItemTypes).GetEnumNames();

		prop.intValue = EditorGUILayout.Popup(prop.displayName, prop.intValue, ItemTypeNames);
	}

	public static void DrawHelpProperty(ref Rect position, SerializedProperty prop, string help)
	{
		DrawHelpBox(ref position, help);

		EditorGUI.PropertyField(position, prop);
		position.y += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
	}

	public static void DrawHelpProperty(ref Rect position, SerializedProperty prop, bool drawChildren, string help)
	{
		DrawHelpBox(ref position, help);

		EditorGUI.PropertyField(position, prop, true);
		position.y += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
	}

	public static void DrawHelpProperty(SerializedProperty prop, string help)
	{
		DrawHelpBox(help);

		EditorGUILayout.PropertyField(prop);
	}

	public static void DrawHelpProperty(SerializedProperty prop, bool drawChildren, string help)
	{
		DrawHelpBox(help);

		EditorGUILayout.PropertyField(prop, drawChildren);
	}

	public static void DrawArrayElement(SerializedProperty arrayProp, int index, string displayName, string help)
	{
		SerializedProperty element = arrayProp.GetArrayElementAtIndex(index);

		DrawHelpBox(help);

		EditorGUILayout.PropertyField(element, new GUIContent(displayName));
	}

	public static void DrawRemainingProperties(SerializedObject obj, SerializedProperty propsAfter)
	{
		SerializedProperty prop = obj.FindProperty(propsAfter.name);

		if (prop != null && prop.NextVisible(false))
		{
			do
				EditorGUILayout.PropertyField(prop);
			while (prop.NextVisible(true));
		}
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

	public static bool GetTranslation(string term, out string result)
	{
		ModLanguageSourceAsset asset = AssetDatabase.LoadAssetAtPath<ModLanguageSourceAsset>(LOCALISATION_ASSET);

		if (asset != null)
		{
			I2.Loc.LanguageSourceAsset gameLang = Resources.Load<I2.Loc.LanguageSourceAsset>("I2Languages");
			
			I2.Loc.TermData termData = asset.mSource.GetTermData(term) ??
				gameLang.mSource.GetTermData(term);

			if (termData == null)
				result = asset.mSource.GetTranslation(term);
			else
				result = termData.GetTranslation(0);

			return result != null;
		}

		result = null;
		return false;
	}

	public static void DrawTranslation(string term, bool isDescription)
	{
		GetTranslation(term, out string result);

		string prefix = isDescription ? "Description " : "";

		EditorGUILayout.LabelField($"{prefix}Localisation Term", term);
		EditorGUILayout.LabelField($"{prefix}Localisation Result", result);
	}

	public static void DrawTranslation(ref Rect position, string term, bool isDescription)
	{
		GetTranslation(term, out string result);

		string prefix = isDescription ? "Description " : "";

		EditorGUI.LabelField(position, $"{prefix}Localisation Term", term);
		position.y += EditorLineSpacing(1);

		EditorGUI.LabelField(position, $"{prefix}Localisation Result", result);
		position.y += EditorLineSpacing(1);
	}

	public static void ReloadLocalisation()
	{
		ModLanguageSourceAsset asset = AssetDatabase.LoadAssetAtPath<ModLanguageSourceAsset>(LOCALISATION_ASSET);

		if (asset != null)
			asset.mSource.UpdateDictionary(true);
	}

	public static void DrawReloadLocalisation()
	{
		if (GUILayout.Button("Reload Localisation"))
			ReloadLocalisation();
	}
}
