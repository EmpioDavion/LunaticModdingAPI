using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModScene), true)]
public class ModSceneEditor : Editor
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
		serializedObject.Update();

		GUI.enabled = false;
		EditorGUILayout.PropertyField(script);
		GUI.enabled = true;

		showHelp = EditorGUILayout.Toggle("Show Help", showHelp);

		if (showHelp)
			EditorGUILayout.HelpBox("You can select a Lunacid scene by name here.", MessageType.Info);

		int index = EditorGUILayout.Popup(sceneName.displayName, -1, scenes);

		if (index != -1)
			sceneName.stringValue = Lunatic.GameScenes.NameToID[scenes[index]];

		EditorTools.DrawHelpProperty(sceneName, "The scene that this ModScene will track and operate on.");

		SerializedProperty prop = serializedObject.GetIterator();

		while (prop.NextVisible(true) && prop.name != sceneName.name)
			;

		while (prop.NextVisible(true))
			EditorGUILayout.PropertyField(prop);

		serializedObject.ApplyModifiedProperties();
	}
}
