using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(I2.Loc.TermData))]
public class TermDataPropertyDrawer : PropertyDrawer
{
	private static readonly List<I2.Loc.LanguageData> Languages;
	private static readonly string[] TermLanguages = new string[]
	{
		"English",
		"Nyan",
		"French",
		"Japanese",
		"Spanish",
		"Russian",
		"Chinese",
		"German",
		"Korean",
		"Polish",
		"Brazilian Portuguese",
		"Spanish LATAM"
	};

	[System.Flags]
	public enum TermFlags
	{
		None			= 0,
		Disabled		= 1
	}

	private delegate void TermDataEntryAction(ref Rect position, SerializedProperty element, int index);

	private static readonly GUIContent DummyContent = new GUIContent("");

	static TermDataPropertyDrawer()
	{
		I2.Loc.LanguageSourceAsset asset = Resources.Load<I2.Loc.LanguageSourceAsset>("I2Languages");

		if (asset != null)
			Languages = asset.SourceData.mLanguages;
		else
			Debug.LogWarning("I2 asset is null");
	}

	private static string GetLanguageName(int index)
	{
		if (Languages == null)
			return TermLanguages[index];

		return Languages[index].Name;
	}

	private static int GetLanguageCount() => Languages?.Count ?? TermLanguages.Length;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (!property.isExpanded)
			return EditorGUIUtility.singleLineHeight;

		SerializedProperty Languages = property.FindPropertyRelative(nameof(Languages));
		SerializedProperty Flags = property.FindPropertyRelative(nameof(Flags));
		SerializedProperty Languages_Touch = property.FindPropertyRelative(nameof(Languages_Touch));

		int lines = 7;

		if (Languages.isExpanded)
			lines += Languages.arraySize;

		if (Flags.isExpanded)
			lines += Flags.arraySize;

		if (Languages_Touch.isExpanded)
			lines += Languages_Touch.arraySize;

		if (EditorTools.ShowHelp)
			lines += 10;

		return lines * EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		position.height = EditorGUIUtility.singleLineHeight;
		property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName);

		if (!property.isExpanded)
			return;

		position.y += EditorGUIUtility.singleLineHeight;

		SerializedProperty Term = property.FindPropertyRelative(nameof(Term));
		SerializedProperty TermType = property.FindPropertyRelative(nameof(TermType));
		SerializedProperty Languages = property.FindPropertyRelative(nameof(Languages));
		SerializedProperty Flags = property.FindPropertyRelative(nameof(Flags));
		SerializedProperty Languages_Touch = property.FindPropertyRelative(nameof(Languages_Touch));

		EditorTools.DrawHelpProperty(ref position, Term, "The ID of the translation term.");
		EditorTools.DrawHelpProperty(ref position, TermType, "The object type of the translation target.");
		DrawEntries(ref position, Languages, DrawLanguage, "Language translation texts.");
		DrawEntries(ref position, Flags, DrawFlag, "If the translation starts disabled.");
		EditorTools.DrawHelpProperty(ref position, Languages_Touch, "Seems to replace values in Languages when the TermData is validated. Unsure of actual use.");
	}

	private void DrawEntries(ref Rect position, SerializedProperty property, TermDataEntryAction action, string help)
	{
		int size = property.arraySize;
		int minSize = GetLanguageCount();

		if (size < minSize)
		{
			property.arraySize = minSize;
			size = minSize;
		}

		EditorTools.DrawHelpBox(ref position, help);

		if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName))
		{
			position.y += EditorGUIUtility.singleLineHeight;

			for (int i = 0; i < size; i++)
				action(ref position, property.GetArrayElementAtIndex(i), i);
		}
		else
			position.y += EditorGUIUtility.singleLineHeight;
	}

	private void DrawLanguage(ref Rect position, SerializedProperty property, int index)
	{
		DummyContent.text = GetLanguageName(index);
		EditorGUI.PropertyField(position, property, DummyContent);
		position.y += EditorGUIUtility.singleLineHeight;
	}

	private void DrawFlag(ref Rect position, SerializedProperty property, int index)
	{
		bool setting = property.intValue == 1;
		bool newSetting = EditorGUI.Toggle(position, GetLanguageName(index), setting);

		if (setting != newSetting)
			property.intValue = newSetting ? 1 : 0;

		position.y += EditorGUIUtility.singleLineHeight;
	}
}
