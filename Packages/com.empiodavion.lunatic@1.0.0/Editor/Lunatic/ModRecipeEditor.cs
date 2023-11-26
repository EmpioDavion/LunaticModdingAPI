using UnityEditor;

[CustomEditor(typeof(ModRecipe), true)]
public class ModRecipeEditor : Editor
{
	protected SerializedProperty description;
	protected SerializedProperty startsUnlocked;

	protected SerializedProperty material1;
	protected SerializedProperty material2;
	protected SerializedProperty material3;

	protected SerializedProperty ingredient1Name;
	protected SerializedProperty ingredient2Name;
	protected SerializedProperty ingredient3Name;

	protected SerializedProperty result;

	private string[] materials;

	private void OnEnable()
	{
		description = serializedObject.FindProperty("description");
		startsUnlocked = serializedObject.FindProperty("startsUnlocked");

		material1 = serializedObject.FindProperty("material1");
		material2 = serializedObject.FindProperty("material2");
		material3 = serializedObject.FindProperty("material3");

		ingredient1Name = serializedObject.FindProperty("ingredient1Name");
		ingredient2Name = serializedObject.FindProperty("ingredient2Name");
		ingredient3Name = serializedObject.FindProperty("ingredient3Name");

		result = serializedObject.FindProperty("result");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		Lunatic.Init();

		if (materials == null)
			materials = Lunatic.MaterialNames.ToArray();

		EditorTools.DrawShowHelpToggle();

		EditorTools.DrawHelpProperty(description, "The description displayed in the alchemy table UI.");
		EditorTools.DrawHelpProperty(startsUnlocked, "If the recipe should already be unlocked on a fresh character.");

		DrawIngredient(material1, ingredient1Name, "The first material needed for the recipe.");
		DrawIngredient(material2, ingredient2Name, "The second material needed for the recipe.");
		DrawIngredient(material3, ingredient3Name, "The third material needed for the recipe.");

		EditorTools.DrawHelpProperty(result, "The item created when the recipe is forged.");

		EditorTools.DrawRemainingProperties(serializedObject, result);

		serializedObject.ApplyModifiedProperties();
	}

	private void DrawIngredient(SerializedProperty prop, SerializedProperty nameProp, string help)
	{
		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox(help, MessageType.Info);

		EditorGUILayout.LabelField("Material");

		EditorGUI.indentLevel++;

		int index = EditorGUILayout.Popup("Select Lunacid Material", -1, materials);

		if (index >= 0)
		{
			prop.objectReferenceValue = null;
			nameProp.stringValue = materials[index];
		}

		EditorTools.DrawRefName(prop, nameProp);

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(prop, new UnityEngine.GUIContent("Reference"));

		if (EditorGUI.EndChangeCheck())
			EditorTools.CheckLunaticRef(prop, nameProp);

		EditorGUI.indentLevel--;
	}
}