using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(ModDialog))]
public class ModDialogEditor : ModBaseEditor
{
	protected SerializedProperty id;
	protected SerializedProperty npcName;
	protected SerializedProperty talkSpeed;
	protected SerializedProperty mouthMult;
	protected SerializedProperty useZ;
	protected SerializedProperty lines;
	protected SerializedProperty mouth;
	protected SerializedProperty eyes;
	protected SerializedProperty objs;
	protected SerializedProperty talkSound;
	protected SerializedProperty objs2;
	protected SerializedProperty jaw;
	protected SerializedProperty music;

	protected ReorderableList linesList;

	public override SerializedProperty LastProperty => id;

	protected virtual void OnEnable()
	{
		id = serializedObject.FindProperty("id");
		npcName = serializedObject.FindProperty("npc_name");
		talkSpeed = serializedObject.FindProperty("talk_speed");
		mouthMult = serializedObject.FindProperty("mouth_mult");
		useZ = serializedObject.FindProperty("use_z");
		lines = serializedObject.FindProperty("LINES");
		mouth = serializedObject.FindProperty("Mouth");
		eyes = serializedObject.FindProperty("Eyes");
		objs = serializedObject.FindProperty("OBJS");
		talkSound = serializedObject.FindProperty("TALK_SND");
		objs2 = serializedObject.FindProperty("OBJS2");
		jaw = serializedObject.FindProperty("Jaw");
		music = serializedObject.FindProperty("MUSIC");

		linesList = new ReorderableList(serializedObject, lines)
		{
			drawElementCallback = DrawLineListMember,
			elementHeightCallback = GetLineListMemberHeight,
			drawHeaderCallback = DrawLineListHeader
		};
	}

	public override void DrawGUI()
	{
		// [0], [1], and [2] are random greetings, [3] is first-time greeting
		if (lines.arraySize < 4)
			lines.arraySize = 4;

		// dialogue text, npc name
		if (objs.arraySize < 2)
			objs.arraySize = 2;

		// yes, no, exit, trigger, shop

		if (objs2.arraySize < 5)
		{
			int size = objs2.arraySize;

			objs2.arraySize = 5;

			if (size < 4)
				objs2.GetArrayElementAtIndex(3).objectReferenceValue = null;

			objs2.GetArrayElementAtIndex(4).objectReferenceValue = null;
		}

		EditorTools.DrawHelpProperty(id, "The ID to use for saving and loading the dialogue state.");
		EditorTools.DrawHelpProperty(npcName, "Name of the NPC.");
		EditorTools.DrawHelpProperty(talkSpeed, "How fast the NPC talks, mouth movement, sound, and dialogue text speed.");
		EditorTools.DrawHelpProperty(music, "The music that plays when talking to the NPC.");

		EditorGUILayout.Space();

		EditorTools.DrawHelpProperty(eyes, "The eyes of the NPC. Keep as null if your NPC does not have at least 2 blend shapes on their eyes renderer.");
		EditorTools.DrawHelpProperty(useZ, "If the eyes blend shape uses the third (Z) value of \"expression\" specified by their lines. Requires a third blend shape.");

		EditorGUILayout.Space();
		
		EditorTools.DrawHelpProperty(mouth, "The mouth of the NPC. Should be set, as it's needed for talking sounds. Does not require blend shapes.");
		EditorTools.DrawHelpProperty(mouthMult, "Scales the mouth blend shape movement, if the mouth has any.");
		EditorTools.DrawHelpProperty(talkSound, "The GameObject that plays talking sounds. Must have an Audio source, and should have a ModRandomSound.");
		EditorTools.DrawHelpProperty(jaw, "The jaw of the NPC, used to animate the mouth when talking. Can be null if you're using blend shapes for the mouth or don't want the mouth to move.");

		EditorGUILayout.Space();

		SerializedProperty trigger = objs2.GetArrayElementAtIndex(3);

		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("Trigger object for dialogue. Typically used for sounds or to change the NPC state.", MessageType.Info);

		EditorGUILayout.PropertyField(trigger, new GUIContent("Trigger"));

		SerializedProperty shop = objs2.GetArrayElementAtIndex(4);

		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("Shop object for dialogue.", MessageType.Info);

		EditorGUILayout.PropertyField(shop, new GUIContent("Shop"));

		EditorGUILayout.Space();

		DrawLine(3, "The initial greeting given the first time the NPC is spoken to. Dialogue state must be zero.");
		DrawLine(0, "Random greeting 1.");
		DrawLine(1, "Random greeting 2.");
		DrawLine(2, "Random greeting 3.");

		EditorGUILayout.Space(5);

		Rect rect = GUILayoutUtility.GetLastRect();
		rect.y += rect.height;

		float height = 80;

		for (int i = 4; i < lines.arraySize; i++)
			height += EditorGUI.GetPropertyHeight(lines.GetArrayElementAtIndex(i));

		EditorGUILayout.Space(height);

		linesList.DoList(rect);
	}

	private void DrawLine(int index, string help)
	{
		SerializedProperty prop = lines.GetArrayElementAtIndex(index);

		if (EditorTools.ShowHelp && !string.IsNullOrEmpty(help))
			EditorGUILayout.HelpBox(help, MessageType.Info);

		EditorGUILayout.PropertyField(prop);

		Rect rect = GUILayoutUtility.GetLastRect();
		rect.height = EditorGUIUtility.singleLineHeight;

		EditorGUI.LabelField(rect, $"{index}:");
	}

	private void DrawLineListMember(Rect rect, int index, bool isActive, bool isFocused)
	{
		if (index >= 4)
		{
			EditorGUI.indentLevel++;

			SerializedProperty prop = lines.GetArrayElementAtIndex(index);
			EditorGUI.PropertyField(rect, prop);

			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(rect, $"{index}:");

			EditorGUI.indentLevel--;
		}
	}

	private float GetLineListMemberHeight(int index)
	{
		if (index >= 4)
		{
			SerializedProperty prop = lines.GetArrayElementAtIndex(index);
			return EditorGUI.GetPropertyHeight(prop);
		}

		return 0.0f;
	}

	private void DrawLineListHeader(Rect rect)
	{
		EditorGUI.LabelField(rect, "Extra Dialogues");
	}
}
