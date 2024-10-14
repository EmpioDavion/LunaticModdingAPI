using UnityEditor;

[CustomEditor(typeof(ModLanguageSourceAsset))]
internal class ModLanguageSourceAssetEditor : ModBaseEditor
{
	private SerializedProperty mSource;

	public override SerializedProperty LastProperty => mSource;

	private void OnEnable()
	{
		mSource = serializedObject.FindProperty(nameof(mSource));
	}

	public override void DrawGUI()
	{
		EditorTools.DrawHelpBox("Check out Packages/Lunatic/Lunacid/Resources/I2Languages for base game translation entries.");

		EditorGUILayout.PropertyField(mSource);
	}
}
