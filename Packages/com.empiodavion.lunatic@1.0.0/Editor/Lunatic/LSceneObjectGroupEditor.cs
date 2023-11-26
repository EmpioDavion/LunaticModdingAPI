using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LSceneObjectGroup), true)]
internal class LSceneObjectGroupEditor : Editor
{
	protected SerializedProperty scene;
	protected SerializedProperty sceneObjects;
	protected SerializedProperty spawnCondition;

	Editor sceneObjectEditor;
	Editor conditionEditor;

	private void OnEnable()
	{
		scene = serializedObject.FindProperty("scene");
		sceneObjects = serializedObject.FindProperty("sceneObjects");
		spawnCondition = serializedObject.FindProperty("spawnCondition");

		SceneView.duringSceneGui -= DrawSceneGUI;
		SceneView.duringSceneGui += DrawSceneGUI;
	}

	private void OnDisable()
	{
		SceneView.duringSceneGui -= DrawSceneGUI;
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(scene);

		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.LabelField(sceneObjects.displayName);

		sceneObjects.arraySize = EditorGUILayout.IntField(sceneObjects.arraySize);

		EditorGUILayout.EndHorizontal();

		EditorGUI.indentLevel++;

		for (int i = 0; i < sceneObjects.arraySize; i++)
		{
			SerializedProperty prop = sceneObjects.GetArrayElementAtIndex(i);

			if (prop.objectReferenceValue == null)
				AssignNewSceneObject(prop);

			if (prop.objectReferenceValue != null)
			{
				CreateCachedEditor(prop.objectReferenceValue, null, ref sceneObjectEditor);
				sceneObjectEditor.OnInspectorGUI();
			}
		}

		EditorGUI.indentLevel--;

		if (spawnCondition.objectReferenceValue == null)
			AssignNewSceneObjectGroupCondition(spawnCondition);


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

	private void DrawSceneGUI(SceneView sceneView)
	{
		LSceneObjectGroup group = (LSceneObjectGroup)target;

		GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.UpperCenter;
		centeredStyle.fixedWidth = 200.0f;
		centeredStyle.normal.background = Texture2D.whiteTexture;
		centeredStyle.normal.textColor = Color.black;

		for (int i = 0; i < group.sceneObjects.Count; i++)
		{
			if (group.sceneObjects[i] == null)
			{
				group.sceneObjects[i] = CreateInstance<LSceneObject>();
				EditorUtility.SetDirty(group);
			}

			LSceneObject sceneObject = group.sceneObjects[i];

			Vector3 pos = sceneObject.localPosition;

			if (!string.IsNullOrEmpty(sceneObject.parentTransform))
			{
				GameObject parent = GameObject.Find(sceneObject.parentTransform);

				if (parent != null)
					pos = parent.transform.TransformPoint(pos);
			}

			Handles.Label(pos + Vector3.up, sceneObject.gameObject == null ? "[NULL]" : sceneObject.gameObject.name, centeredStyle);
			Handles.DrawWireCube(pos, Vector3.one);
		}
	}
}
