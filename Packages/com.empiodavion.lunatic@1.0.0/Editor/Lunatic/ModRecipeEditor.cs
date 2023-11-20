using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModRecipe), true)]
public class ModRecipeEditor : Editor
{
	protected SerializedProperty description;
	protected SerializedProperty startsUnlocked;
	protected SerializedProperty ingredient1Name;
	protected SerializedProperty ingredient2Name;
	protected SerializedProperty ingredient3Name;
	protected SerializedProperty result;

	private void OnEnable()
	{
		description = serializedObject.FindProperty("description");
		startsUnlocked = serializedObject.FindProperty("startsUnlocked");
		ingredient1Name = serializedObject.FindProperty("ingredient1Name");
		ingredient2Name = serializedObject.FindProperty("ingredient2Name");
		ingredient3Name = serializedObject.FindProperty("ingredient3Name");
		result = serializedObject.FindProperty("result");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorTools.DrawShowHelpToggle();

		EditorTools.DrawHelpProperty(description, "The description displayed in the alchemy table UI.");
		EditorTools.DrawHelpProperty(startsUnlocked, "If the recipe should already be unlocked on a fresh character.");

		DrawIngredient(ingredient1Name, "The name of the first material needed for the recipe.");
		DrawIngredient(ingredient2Name, "The name of the second material needed for the recipe.");
		DrawIngredient(ingredient3Name, "The name of the third material needed for the recipe.");

		EditorTools.DrawHelpProperty(result, "The item created when the recipe is forged.");

		EditorTools.DrawRemainingProperties(serializedObject, result);

		serializedObject.ApplyModifiedProperties();
	}

	private void DrawIngredient(SerializedProperty prop, string help)
	{
		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox(help, MessageType.Info);

		EditorGUI.BeginChangeCheck();

		Object obj = EditorGUILayout.ObjectField("Copy object name", null, typeof(ModMaterial), false);

		if (EditorGUI.EndChangeCheck() && obj != null)
			prop.stringValue = obj.name;

		EditorGUILayout.PropertyField(prop);
	}
}