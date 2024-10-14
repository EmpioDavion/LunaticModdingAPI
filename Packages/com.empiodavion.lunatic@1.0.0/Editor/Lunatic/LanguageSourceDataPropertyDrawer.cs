using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(I2.Loc.LanguageSourceData))]
internal class LanguageSourceDataPropertyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (!property.isExpanded)
			return EditorGUIUtility.singleLineHeight;

		SerializedProperty mTerms = property.FindPropertyRelative(nameof(mTerms));
		SerializedProperty CaseInsensitiveTerms = property.FindPropertyRelative(nameof(CaseInsensitiveTerms));
		SerializedProperty mLanguages = property.FindPropertyRelative(nameof(mLanguages));

		float termsHeight = EditorGUI.GetPropertyHeight(mTerms);
		float languagesHeight = EditorGUI.GetPropertyHeight(mLanguages);

		return EditorGUIUtility.singleLineHeight * 12 + termsHeight + languagesHeight +
			EditorGUI.GetPropertyHeight(CaseInsensitiveTerms) * 3;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		position.height = EditorGUIUtility.singleLineHeight;

		if (!(property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label)))
			return;

		EditorGUI.indentLevel++;

		position.y += EditorGUIUtility.singleLineHeight;

		SerializedProperty mTerms = property.FindPropertyRelative(nameof(mTerms));
		SerializedProperty CaseInsensitiveTerms = property.FindPropertyRelative(nameof(CaseInsensitiveTerms));
		SerializedProperty OnMissingTranslation = property.FindPropertyRelative(nameof(OnMissingTranslation));
		SerializedProperty mLanguages = property.FindPropertyRelative(nameof(mLanguages));
		SerializedProperty IgnoreDeviceLanguage = property.FindPropertyRelative(nameof(IgnoreDeviceLanguage));

		EditorTools.DrawHelpProperty(ref position, CaseInsensitiveTerms, "Whether it is possible to get the localisation entry even if the term does not match lower/upper case. Currently required for descriptions as the capitalisation is not consistent in Lunacid's code.");
		EditorTools.DrawHelpProperty(ref position, IgnoreDeviceLanguage, "If the initial language should not be set to the use the language the player's computer is set to.");
		EditorTools.DrawHelpProperty(ref position, OnMissingTranslation, "How the text should be modified if the term does not exist.");
		EditorTools.DrawHelpProperty(ref position, mLanguages, true, "The list of languages available for localisation.");
		EditorTools.DrawHelpProperty(ref position, mTerms, true, "The list of localisation entries.");

		EditorGUI.indentLevel--;
	}
}
