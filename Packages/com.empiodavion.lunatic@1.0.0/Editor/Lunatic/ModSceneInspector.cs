using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModScene), true)]
public class ModSceneInspector : Editor
{
	public bool showHelp = true;

	protected string[] scenes;

	private SerializedProperty script;
	private SerializedProperty sceneName;

	private void OnEnable()
	{
		scenes = Lunatic.GameScenes.NameToID.Keys.ToArray();

		script = serializedObject.FindProperty("m_Script");
		sceneName = serializedObject.FindProperty("sceneName");
	}

	public override void OnInspectorGUI()
	{
		GUI.enabled = false;
		EditorGUILayout.PropertyField(script);
		GUI.enabled = true;

		showHelp = EditorGUILayout.Toggle("Show Help", showHelp);

		if (showHelp)
			EditorGUILayout.HelpBox("The scene that this ModScene will track and operate on.", MessageType.Info);

		//int index = GUILayout.drop(sceneName.displayName, scenes);

		SerializedProperty prop = serializedObject.GetIterator();

		while (prop.NextVisible(true) && prop.name != sceneName.name)
			;

		while (prop.NextVisible(true))
			EditorGUILayout.PropertyField(prop);
	}
}
