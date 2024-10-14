using UnityEditor;

[CustomEditor(typeof(ModLocalise), true)]
public class ModLocaliseEditor : ModBaseEditor
{
	#region Properties

	public SerializedProperty mTerm;
	public SerializedProperty mTermSecondary;
	public SerializedProperty PrimaryTermModifier;
	public SerializedProperty SecondaryTermModifier;
	public SerializedProperty TermPrefix;
	public SerializedProperty TermSuffix;
	public SerializedProperty LocalizeOnAwake;
	public SerializedProperty IgnoreRTL;
	public SerializedProperty MaxCharactersInRTL;
	public SerializedProperty IgnoreNumbersInRTL;
	public SerializedProperty CorrectAlignmentForRTL;
	public SerializedProperty AddSpacesToJoinedLanguages;
	public SerializedProperty AllowLocalizedParameters;
	public SerializedProperty AllowParameters;
	public SerializedProperty TranslatedObjects;
	public SerializedProperty LocalizeEvent;
	public SerializedProperty AlwaysForceLocalize;
	public SerializedProperty LocalizeCallBack;
	public SerializedProperty mGUI_ShowReferences;
	public SerializedProperty mGUI_ShowTems;
	public SerializedProperty mGUI_ShowCallback;
	public SerializedProperty mLocalizeTarget;
	public SerializedProperty mLocalizeTargetName;

	#endregion

	public override SerializedProperty LastProperty => mLocalizeTargetName;

	private void OnEnable()
	{
		mTerm = serializedObject.FindProperty(nameof(mTerm));
		mTermSecondary = serializedObject.FindProperty(nameof(mTermSecondary));
		PrimaryTermModifier = serializedObject.FindProperty(nameof(PrimaryTermModifier));
		SecondaryTermModifier = serializedObject.FindProperty(nameof(SecondaryTermModifier));
		TermPrefix = serializedObject.FindProperty(nameof(TermPrefix));
		TermSuffix = serializedObject.FindProperty(nameof(TermSuffix));
		LocalizeOnAwake = serializedObject.FindProperty(nameof(LocalizeOnAwake));
		IgnoreRTL = serializedObject.FindProperty(nameof(IgnoreRTL));
		MaxCharactersInRTL = serializedObject.FindProperty(nameof(MaxCharactersInRTL));
		IgnoreNumbersInRTL = serializedObject.FindProperty(nameof(IgnoreNumbersInRTL));
		CorrectAlignmentForRTL = serializedObject.FindProperty(nameof(CorrectAlignmentForRTL));
		AddSpacesToJoinedLanguages = serializedObject.FindProperty(nameof(AddSpacesToJoinedLanguages));
		AllowLocalizedParameters = serializedObject.FindProperty(nameof(AllowLocalizedParameters));
		AllowParameters = serializedObject.FindProperty(nameof(AllowParameters));
		TranslatedObjects = serializedObject.FindProperty(nameof(TranslatedObjects));
		LocalizeEvent = serializedObject.FindProperty(nameof(LocalizeEvent));
		AlwaysForceLocalize = serializedObject.FindProperty(nameof(AlwaysForceLocalize));
		LocalizeCallBack = serializedObject.FindProperty(nameof(LocalizeCallBack));
		mGUI_ShowReferences = serializedObject.FindProperty(nameof(mGUI_ShowReferences));
		mGUI_ShowTems = serializedObject.FindProperty(nameof(mGUI_ShowTems));
		mGUI_ShowCallback = serializedObject.FindProperty(nameof(mGUI_ShowCallback));
		mLocalizeTarget = serializedObject.FindProperty(nameof(mLocalizeTarget));
		mLocalizeTargetName = serializedObject.FindProperty(nameof(mLocalizeTargetName));
}

	public override void DrawGUI()
	{
		EditorTools.DrawHelpProperty(mTerm, "The ID for the first translation.");
		EditorTools.DrawHelpProperty(mTermSecondary, "The ID for the second translation, usually for a description.");
		EditorTools.DrawHelpProperty(PrimaryTermModifier, "How the resulting first translation text should be modified.");
		EditorTools.DrawHelpProperty(SecondaryTermModifier, "How the resulting second translation text should be modified.");
		EditorTools.DrawHelpProperty(TermPrefix, "Text that should be added before the first translation text.");
		EditorTools.DrawHelpProperty(TermSuffix, "Text that should be added after the first translation text.");
		EditorTools.DrawHelpProperty(LocalizeOnAwake, "If the localisation should be done when this object first activates.");
		EditorTools.DrawHelpProperty(IgnoreRTL, "If the prefix and suffix should not swap sides for languages that are read right-to-left.");
		EditorTools.DrawHelpProperty(MaxCharactersInRTL, "The maximum characters per line for languages that are read right-to-left.");
		EditorTools.DrawHelpProperty(IgnoreNumbersInRTL, "If numbers should not be translated, currently only affects Hindi.");
		EditorTools.DrawHelpProperty(CorrectAlignmentForRTL, "If the text should be aligned differently for languages that are read right-to-left. Alignment specification is set on the target.");
		EditorTools.DrawHelpProperty(AddSpacesToJoinedLanguages, "Adds spaces between every character of the translation.");
		EditorTools.DrawHelpProperty(AllowLocalizedParameters, "Allow replacement of text blocks in the translation by TermData. {[Example]}");
		EditorTools.DrawHelpProperty(AllowParameters, "Allow replacement of text blocks in the translation by ILocalizationParamsManager. {[Example]}");
		EditorTools.DrawHelpProperty(TranslatedObjects, "The list of objects that should be translated.");
		EditorTools.DrawHelpProperty(LocalizeEvent, false, "Methods to run when localisation occurs.");
		EditorTools.DrawHelpProperty(AlwaysForceLocalize, "If the localisation should run even if the language hasn't changed.");
		EditorTools.DrawHelpProperty(LocalizeCallBack, "Run a particular method on a component when localisation occurs.");
		//EditorTools.DrawHelpProperty(mGUI_ShowReferences, "Debug toggle?");
		//EditorTools.DrawHelpProperty(mGUI_ShowTems, "Debug toggle?");
		//EditorTools.DrawHelpProperty(mGUI_ShowCallback, "Debug toggle?");
		EditorTools.DrawHelpProperty(mLocalizeTarget, "The object to localise. Can be located at runtime.");
		EditorTools.DrawHelpProperty(mLocalizeTargetName, "The full type name of the object to localise.");
}
}
