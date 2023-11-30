using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModScene), true)]
public class ModSceneEditor : ModBaseEditor
{
	protected string[] scenes;

	private SerializedProperty sceneName;

	public override SerializedProperty LastProperty => sceneName;

	private void OnEnable()
	{
		scenes = Lunatic.GameScenes.NameToID.Keys.ToArray();

		sceneName = serializedObject.FindProperty("sceneName");
	}

	public override void DrawGUI()
	{
		if (EditorTools.ShowHelp)
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
	}
}
