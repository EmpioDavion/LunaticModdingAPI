﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LSceneObjectGroup), true)]
internal class LSceneObjectGroupEditor : ModBaseEditor
{
	protected string[] scenes;

	protected SerializedProperty scene;
	protected SerializedProperty sceneObjects;
	protected SerializedProperty spawnCondition;

	private readonly Dictionary<Object, Editor> sceneObjectEditors = new Dictionary<Object, Editor>();
	private Editor conditionEditor;

	public override SerializedProperty LastProperty => spawnCondition;

	private void OnEnable()
	{
		scenes = Lunatic.GameScenes.NameToID.Keys.ToArray();

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

	public override void DrawGUI()
	{
		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("You can select a Lunacid scene by name here.", MessageType.Info);

		int index = EditorGUILayout.Popup(scene.displayName, -1, scenes);

		if (index != -1)
			scene.stringValue = Lunatic.GameScenes.NameToID[scenes[index]];

		EditorTools.DrawHelpProperty(scene, "The scene that this LSceneObjectGroup will spawn into.");

		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("The LSceneObjects that will spawn into the scene under certain conditions.", MessageType.Info);

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

					LSceneObjectEditor sceneObjectEditor = (LSceneObjectEditor)editor;
					sceneObjectEditor.drawHeader = false;
					sceneObjectEditor.expectedScene = scene.stringValue;
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
