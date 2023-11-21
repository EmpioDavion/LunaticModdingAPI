using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LSceneObjectGroup.SceneObject))]
public class SceneObjectPropertyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUIUtility.singleLineHeight * 6;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		position.height = EditorGUIUtility.singleLineHeight;

		EditorGUI.LabelField(position, property.displayName);

		position.y += EditorGUIUtility.singleLineHeight;

		EditorGUI.indentLevel++;

		SerializedProperty scene = property.serializedObject.FindProperty("scene");
		SerializedProperty gameObject = property.FindPropertyRelative("gameObject");
		SerializedProperty positionProp = property.FindPropertyRelative("localPosition");
		SerializedProperty rotation = property.FindPropertyRelative("localRotation");
		SerializedProperty scale = property.FindPropertyRelative("localScale");
		SerializedProperty parentTransform = property.FindPropertyRelative("parentTransform");

		EditorGUI.BeginChangeCheck();

		GameObject newGameObject = EditorGUI.ObjectField(position, gameObject.objectReferenceValue, typeof(GameObject), true) as GameObject;

		position.y += EditorGUIUtility.singleLineHeight;

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
						scene.stringValue = newGameObject.scene.name;

						Transform tr = newGameObject.transform;
						positionProp.vector3Value = tr.localPosition;
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

		EditorGUI.PropertyField(position, positionProp);

		position.y += EditorGUIUtility.singleLineHeight;

		EditorGUI.PropertyField(position, rotation);

		position.y += EditorGUIUtility.singleLineHeight;

		GUI.enabled = false;

		EditorGUI.PropertyField(position, parentTransform);

		GUI.enabled = true;

		EditorGUI.indentLevel--;
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
