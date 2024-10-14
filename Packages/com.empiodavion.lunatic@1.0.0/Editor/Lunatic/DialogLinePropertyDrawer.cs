using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Dialog.Line))]
public class DialogLinePropertyDrawer : PropertyDrawer
{
	private static readonly Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		Foldouts.TryGetValue(property.propertyPath, out bool foldout);
		
		if (!foldout)
			return EditorTools.EditorHeight(1);

		int lines = 10;

		if (EditorTools.ShowHelp)
			lines += 12;

		SerializedProperty special = property.FindPropertyRelative("special");

		if (special.intValue == (int)ModDialog.DialogResponses.YesNo)
			lines++;

		return EditorTools.EditorHeight(lines);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		position.height = EditorGUIUtility.singleLineHeight;

		SerializedProperty lines = property.serializedObject.FindProperty("LINES");
		SerializedProperty npcName = property.serializedObject.FindProperty("npc_name");

		SerializedProperty value = property.FindPropertyRelative("value");
		SerializedProperty nxt = property.FindPropertyRelative("NXT");
		SerializedProperty newSaid = property.FindPropertyRelative("NEW_SAID");
		SerializedProperty expression = property.FindPropertyRelative("exprssion");
		SerializedProperty loadLine = property.FindPropertyRelative("LOAD_LINE");
		SerializedProperty special = property.FindPropertyRelative("special");

		Foldouts.TryGetValue(property.propertyPath, out bool foldout);

		foldout = EditorGUI.Foldout(position, foldout, "      Dialogue Line: " + property.displayName);

		Foldouts[property.propertyPath] = foldout;

		if (foldout)
		{
			position.y += EditorTools.EditorLineSpacing(1);

			EditorGUI.indentLevel++;

			EditorTools.DrawHelpProperty(ref position, value, "The dialogue text to display. GETS REPLACED WITH LOCALISATION.");

			position.y += EditorTools.EditorLineSpacing(1);

			DrawNextLines(ref position, lines, nxt, special.intValue);

			position.y += EditorTools.EditorLineSpacing(1);

			DrawNewSaid(ref position, lines, newSaid, nxt);

			position.y += EditorTools.EditorLineSpacing(1);

			EditorTools.DrawHelpProperty(ref position, expression, "The blend shape percentage values for eyes.");

			string path = loadLine.propertyPath;
			int open = path.LastIndexOf('[');
			int close = path.LastIndexOf("]");
			int index = 0;

			if (open >= 0 && close >= 0)
				int.TryParse(path.Substring(open + 1, close - open - 1), out index);

			DrawLoadLine(ref position, npcName, loadLine, index);

			if (EditorTools.ShowHelp)
			{
				Rect help = position;
				help.height = EditorGUIUtility.singleLineHeight * 2;
				EditorGUI.HelpBox(help, "The player response options for this dialogue.", MessageType.Info);
				position.y += EditorTools.EditorLineSpacing(2);
			}

			EditorGUI.BeginChangeCheck();

			ModDialog.DialogResponses response = (ModDialog.DialogResponses)EditorGUI.EnumPopup(position, (ModDialog.DialogResponses)special.intValue);

			if (EditorGUI.EndChangeCheck())
				special.intValue = (int)response;

			EditorGUI.indentLevel--;
		}
	}

	private void DrawNextLines(ref Rect position, SerializedProperty lines, SerializedProperty nxt, int special)
	{
		Rect pos = position;

		if (special == (int)ModDialog.DialogResponses.YesNo)
		{
			if (EditorTools.ShowHelp)
			{
				Rect help = position;
				help.height = EditorGUIUtility.singleLineHeight * 2;
				EditorGUI.HelpBox(help, "Response dialogs", MessageType.Info);
				position.y += EditorTools.EditorLineSpacing(2);
			}

			position.width /= 2;

			nxt.intValue = EditorGUI.IntField(position, "Yes", nxt.intValue);

			position.x += position.width;

			if (nxt.intValue >= 0 && nxt.intValue < lines.arraySize)
			{
				SerializedProperty line = lines.GetArrayElementAtIndex(nxt.intValue);
				EditorGUI.LabelField(position, line.displayName);
			}
			else
				EditorGUI.LabelField(position, "INDEX NOT VALID.");

			position.x = pos.x;
			position.y += EditorTools.EditorLineSpacing(1);

			GUI.enabled = false;

			EditorGUI.IntField(position, "No", nxt.intValue + 1);

			GUI.enabled = true;

			position.x += position.width;

			if (nxt.intValue >= -1 && nxt.intValue < lines.arraySize - 1)
			{
				SerializedProperty line = lines.GetArrayElementAtIndex(nxt.intValue + 1);
				EditorGUI.LabelField(position, line.displayName);
			}
			else
				EditorGUI.LabelField(position, "INDEX NOT VALID.");
		}
		else
		{
			if (EditorTools.ShowHelp)
			{
				Rect help = position;
				help.height = EditorGUIUtility.singleLineHeight * 2;
				EditorGUI.HelpBox(help, "The next dialogue after this one. Set to -1 for exiting dialogue.", MessageType.Info);
				position.y += EditorTools.EditorLineSpacing(2);
			}

			position.width /= 2;

			EditorGUI.PropertyField(position, nxt);

			position.x += position.width;

			if (nxt.intValue == -1)
			{
				EditorGUI.LabelField(position, "EXIT DIALOGUE");
			}
			else if (nxt.intValue >= 0 && nxt.intValue < lines.arraySize)
			{
				SerializedProperty line = lines.GetArrayElementAtIndex(nxt.intValue);
				EditorGUI.LabelField(position, line.displayName);
			}
			else
				EditorGUI.LabelField(position, "INDEX NOT VALID.");
		}
		
		position.x = pos.x;
		position.width = pos.width;
	}

	private void DrawNewSaid(ref Rect position, SerializedProperty lines, SerializedProperty newSaid, SerializedProperty nxt)
	{
		Rect pos = position;

		if (EditorTools.ShowHelp)
		{
			Rect help = position;
			help.height = EditorGUIUtility.singleLineHeight * 2;
			EditorGUI.HelpBox(help, "Sets the line for after greetings if this is an exiting dialogue. Stores value in ModDialog if higher than stored. Won't store if this is the opening dialogue.", MessageType.Info);
			position.y += EditorTools.EditorLineSpacing(2);
		}

		position.width /= 2;

		EditorGUI.PropertyField(position, newSaid);

		position.x += position.width;

		if (newSaid.intValue == -1)
		{
			EditorGUI.LabelField(position, "EXIT DIALOGUE");
		}
		else if (newSaid.intValue >= 5 && newSaid.intValue < lines.arraySize)
		{
			SerializedProperty line = lines.GetArrayElementAtIndex(newSaid.intValue);
			EditorGUI.LabelField(position, line.displayName);
		}
		else if (newSaid.intValue >= 0 && newSaid.intValue < 5)
		{
			if (nxt.intValue == -1)
				EditorGUI.LabelField(position, "INDEX MAY CAUSE DIALOGUE LOOP.");
		}
		else
			EditorGUI.LabelField(position, "INDEX NOT VALID.");

		position.x = pos.x;
		position.width = pos.width;
	}

	private void DrawLoadLine(ref Rect position, SerializedProperty npcName,
		SerializedProperty loadLine, int index)
	{
		EditorTools.DrawHelpProperty(ref position, loadLine, "Localisation ID for this line.");

		int id = loadLine.intValue;
		string term = $"Dialog/{npcName.stringValue} {(id > 0 ? id : index)}";

		EditorTools.DrawTranslation(ref position, term, false);
	}
}
