using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LSceneObject), true)]
public class LSceneObjectEditor : Editor
{
	protected SerializedProperty gameObject;
	protected SerializedProperty position;
	protected SerializedProperty rotation;
	protected SerializedProperty scale;
	protected SerializedProperty parentTransform;
	protected SerializedProperty spawnCondition;

	protected Editor spawnConditionEditor;

	private void OnEnable()
	{
		gameObject = serializedObject.FindProperty("gameObject");
		position = serializedObject.FindProperty("localPosition");
		rotation = serializedObject.FindProperty("localRotation");
		scale = serializedObject.FindProperty("localScale");
		parentTransform = serializedObject.FindProperty("parentTransform");
		spawnCondition = serializedObject.FindProperty("spawnCondition");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.LabelField(target.name);

		EditorGUI.indentLevel++;

		EditorGUI.BeginChangeCheck();

		GameObject newGameObject = EditorGUILayout.ObjectField(gameObject.objectReferenceValue, typeof(GameObject), true) as GameObject;

		if (EditorGUI.EndChangeCheck())
		{
			if (newGameObject != null)
			{
				if (!AssetDatabase.Contains(newGameObject))
				{
					GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(newGameObject);

					if (prefab == null)
					{
						Debug.LogError($"Object \"{newGameObject.name}\" given must be a prefab or prefab instance");
						newGameObject = gameObject.objectReferenceValue as GameObject;
					}
					else
					{
						Transform tr = newGameObject.transform;
						position.vector3Value = tr.localPosition;
						rotation.vector3Value = tr.localRotation.eulerAngles;
						scale.vector3Value = tr.localScale;
						parentTransform.stringValue = GetScenePath(tr.parent);

						Debug.LogWarning($"Setting gameObject value of SceneObject to the prefab asset of \"{newGameObject.name}\"");
						newGameObject = prefab;
					}
				}
			}

			gameObject.objectReferenceValue = newGameObject;
		}

		EditorGUILayout.PropertyField(position);

		EditorGUILayout.PropertyField(rotation);

		GUI.enabled = false;

		EditorGUILayout.PropertyField(parentTransform);

		GUI.enabled = true;

		if (spawnCondition.objectReferenceValue == null)
		{
			System.IO.Directory.CreateDirectory("Assets/Conditions");
			spawnCondition.objectReferenceValue = EditorTools.CreateNewAsset<LSceneObjectCondition>("Assets/Conditions/Scene Object Condition");
			EditorUtility.SetDirty(target);
		}

		CreateCachedEditor(spawnCondition.objectReferenceValue, typeof(LConditionBaseEditor), ref spawnConditionEditor);

		spawnConditionEditor.OnInspectorGUI();

		EditorGUI.indentLevel--;

		serializedObject.ApplyModifiedProperties();
	}

	protected string GetScenePath(Transform tr)
	{
		if (tr == null)
			return "";

		string name = tr.name;

		while (tr.parent != null)
		{
			tr = tr.parent;
			name = $"{tr.name}/{name}";
		}

		return name;
	}
}
