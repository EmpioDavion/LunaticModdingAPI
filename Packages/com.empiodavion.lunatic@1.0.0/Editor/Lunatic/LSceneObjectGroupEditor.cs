using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LSceneObjectGroup), true)]
internal class LSceneObjectGroupEditor : Editor
{
	protected SerializedProperty scene;
	protected SerializedProperty sceneObjects;
	protected SerializedProperty spawnCondition;

	private Dictionary<Object, Editor> sceneObjectEditors = new Dictionary<Object, Editor>();
	private Editor conditionEditor;

	private void OnEnable()
	{
		scene = serializedObject.FindProperty("scene");
		sceneObjects = serializedObject.FindProperty("sceneObjects");
		spawnCondition = serializedObject.FindProperty("spawnCondition");
	}

	private void OnDisable()
	{
		foreach (KeyValuePair<Object, Editor> kvp in sceneObjectEditors)
			if (kvp.Value is LSceneObjectEditor editor)
				editor.OnDisable();

		sceneObjectEditors.Clear();
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<Object, Editor> kvp in sceneObjectEditors)
			if (kvp.Value is LSceneObjectEditor editor)
				editor.OnDisable();

		sceneObjectEditors.Clear();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(scene);

		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.LabelField(sceneObjects.displayName);

		sceneObjects.arraySize = EditorGUILayout.IntField(sceneObjects.arraySize, GUILayout.Width(40));

		EditorGUILayout.EndHorizontal();

		EditorGUI.indentLevel++;

		for (int i = 0; i < sceneObjects.arraySize; i++)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);

			SerializedProperty prop = sceneObjects.GetArrayElementAtIndex(i);

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.PropertyField(prop);

			if (GUILayout.Button("Create", GUILayout.Width(100)))
			{
				AssignNewSceneObject(prop);

				EditorTools.CopyAssetBundleLabel(target, prop.objectReferenceValue);
			}

			bool delete = GUILayout.Button("X", GUILayout.Width(20));

			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;

			if (prop.objectReferenceValue != null)
			{
				if (!sceneObjectEditors.TryGetValue(prop.objectReferenceValue, out Editor editor))
				{
					CreateCachedEditor(prop.objectReferenceValue, typeof(LSceneObjectEditor), ref editor);
					sceneObjectEditors.Add(prop.objectReferenceValue, editor);
				}

				editor.OnInspectorGUI();
			}

			if (delete)
			{
				prop.objectReferenceValue = null;
				sceneObjects.DeleteArrayElementAtIndex(i--);
			}

			EditorGUI.indentLevel--;

			EditorGUILayout.EndVertical();
		}

		EditorGUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();

		if (GUILayout.Button("+", GUILayout.Width(64)))
		{
			sceneObjects.arraySize++;

			SerializedProperty prop = sceneObjects.GetArrayElementAtIndex(sceneObjects.arraySize - 1);
			prop.objectReferenceValue = null;
		}

		EditorGUILayout.EndHorizontal();

		EditorGUI.indentLevel--;

		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.PropertyField(spawnCondition);

		if (GUILayout.Button("Create", GUILayout.Width(100)))
		{
			AssignNewSceneObjectGroupCondition(spawnCondition);

			EditorTools.CopyAssetBundleLabel(target, spawnCondition.objectReferenceValue);
		}

		if (GUILayout.Button("X", GUILayout.Width(20)))
			spawnCondition.objectReferenceValue = null;

		EditorGUILayout.EndHorizontal();

		if (spawnCondition.objectReferenceValue != null)
		{
			CreateCachedEditor(spawnCondition.objectReferenceValue, typeof(LConditionBaseEditor), ref conditionEditor);
			conditionEditor.OnInspectorGUI();
		}

		EditorTools.DrawRemainingProperties(serializedObject, spawnCondition);

		serializedObject.ApplyModifiedProperties();
	}

	private void AssignNewSceneObject(SerializedProperty property)
	{
		Directory.CreateDirectory("Assets/SceneObjects");
		property.objectReferenceValue = EditorTools.CreateNewAsset<LSceneObject>("Assets/SceneObjects/Scene Object");
		EditorUtility.SetDirty(target);
	}

	private void AssignNewSceneObjectGroupCondition(SerializedProperty property)
	{
		Directory.CreateDirectory("Assets/Conditions");
		property.objectReferenceValue = EditorTools.CreateNewAsset<LSceneObjectGroupCondition>("Assets/Conditions/Scene Object Group Condition");
		EditorUtility.SetDirty(target);
	}
}
