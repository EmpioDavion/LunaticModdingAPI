using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(LSceneObject), true)]
public class LSceneObjectEditor : ModBaseEditor
{
	private static readonly Dictionary<Object, Editor> Editors = new Dictionary<Object, Editor>();

	protected SerializedProperty gameObject;
	protected SerializedProperty position;
	protected SerializedProperty rotation;
	protected SerializedProperty scale;
	protected SerializedProperty parentTransform;
	protected SerializedProperty spawnCondition;

	protected Editor spawnConditionEditor;

	public string expectedScene;

	public override SerializedProperty LastProperty => spawnCondition;

	private void OnEnable()
	{
		if (target == null)
		{
			DestroyImmediate(this);
			return;
		}

		Editors[serializedObject.targetObject] = this;

		gameObject = serializedObject.FindProperty("gameObject");
		position = serializedObject.FindProperty("localPosition");
		rotation = serializedObject.FindProperty("localRotation");
		scale = serializedObject.FindProperty("localScale");
		parentTransform = serializedObject.FindProperty("parentTransform");
		spawnCondition = serializedObject.FindProperty("spawnCondition");

		SceneView.duringSceneGui -= DrawSceneGUI;
		SceneView.duringSceneGui += DrawSceneGUI;

		SceneView.RepaintAll();
	}

	public void OnDisable()
	{
		SceneView.duringSceneGui -= DrawSceneGUI;
	}

	public void OnDestroy()
	{
		SceneView.duringSceneGui -= DrawSceneGUI;
	}

	public override void DrawGUI()
	{
		EditorGUILayout.LabelField(target.name);

		EditorGUI.indentLevel++;

		EditorGUI.BeginChangeCheck();

		GameObject newGameObject = EditorGUILayout.ObjectField(gameObject.displayName, gameObject.objectReferenceValue, typeof(GameObject), true) as GameObject;

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

		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.PropertyField(spawnCondition);

		if (GUILayout.Button("Create", GUILayout.Width(100)))
		{
			AssignNewSceneObjectCondition(spawnCondition);

			EditorTools.CopyAssetBundleLabel(target, spawnCondition.objectReferenceValue);
		}

		if (GUILayout.Button("X", GUILayout.Width(20)))
			spawnCondition.objectReferenceValue = null;

		EditorGUILayout.EndHorizontal();

		if (spawnCondition.objectReferenceValue != null)
		{
			CreateCachedEditor(spawnCondition.objectReferenceValue, typeof(LConditionBaseEditor), ref spawnConditionEditor);

			LConditionBaseEditor conditionEditor = (LConditionBaseEditor)spawnConditionEditor;
			conditionEditor.drawHeader = false;

			spawnConditionEditor.OnInspectorGUI();
		}

		EditorGUI.indentLevel--;
	}

	private void AssignNewSceneObjectCondition(SerializedProperty property)
	{
		System.IO.Directory.CreateDirectory("Assets/Conditions");
		property.objectReferenceValue = EditorTools.CreateNewAsset<LSceneObjectCondition>("Assets/Conditions/Scene Object Condition");
		EditorUtility.SetDirty(target);
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

	private void DrawSceneGUI(SceneView sceneView)
	{
		if (PrefabStageUtility.GetCurrentPrefabStage() != null)
			return;

		if (!string.IsNullOrEmpty(expectedScene) && expectedScene != SceneManager.GetActiveScene().name)
			return;

		serializedObject.Update();

		LSceneObject sceneObject = (LSceneObject)target;

		GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.UpperCenter;
		centeredStyle.fixedWidth = 200.0f;
		centeredStyle.normal.background = Texture2D.whiteTexture;
		centeredStyle.normal.textColor = Color.black;

		Vector3 pos = position.vector3Value;
		Quaternion rot = Quaternion.Euler(rotation.vector3Value);
		Vector3 sca = scale.vector3Value;
		Transform parentTr = null;

		if (!string.IsNullOrEmpty(sceneObject.parentTransform))
		{
			GameObject parent = GameObject.Find(sceneObject.parentTransform);

			if (parent != null)
			{
				parentTr = parent.transform;

				pos = parentTr.TransformPoint(pos);
				rot = parentTr.rotation * rot;
			}
		}

		EditorGUI.BeginChangeCheck();

		if (Tools.current == Tool.Move)
		{
			pos = Handles.PositionHandle(pos, rot);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(sceneObject, "Moved Scene Object");

				if (parentTr != null)
					position.vector3Value = parentTr.InverseTransformPoint(pos);
				else
					position.vector3Value = pos;

				EditorUtility.SetDirty(target);
				Repaint();
			}
		}
		else if (Tools.current == Tool.Rotate)
		{
			rot = Handles.RotationHandle(rot, pos);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(sceneObject, "Rotated Scene Object");

				if (parentTr != null)
					rotation.vector3Value = (Quaternion.Inverse(parentTr.rotation) * rot).eulerAngles;
				else
					rotation.vector3Value = rot.eulerAngles;

				EditorUtility.SetDirty(target);
				Repaint();
			}
		}
		else if (Tools.current == Tool.Scale)
		{
			sca = Handles.ScaleHandle(sca, pos, rot, 1.0f);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(sceneObject, "Scaled Scene Object");

				scale.vector3Value = sca;

				EditorUtility.SetDirty(target);
				Repaint();
			}
		}

		Handles.Label(pos + Vector3.up, sceneObject.gameObject == null ? "[NULL]" : sceneObject.gameObject.name, centeredStyle);

		Handles.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);

		Handles.DrawWireCube(Vector3.zero, Vector3.one);

		Handles.matrix = Matrix4x4.identity;

		if (!Editors.TryGetValue(target, out Editor editor) || editor != this)
			SceneView.duringSceneGui -= DrawSceneGUI;

		serializedObject.ApplyModifiedProperties();
	}
}
