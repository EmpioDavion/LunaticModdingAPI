using UnityEditor;

[CustomEditor(typeof(ModSpawnOnEnable), true)]
public class ModSpawnOnEnableEditor : ModBaseEditor
{
	protected SerializedProperty item;
	protected SerializedProperty npcMagic;
	protected SerializedProperty weapon;
	protected SerializedProperty power;
	protected SerializedProperty forward;
	protected SerializedProperty Override;
	protected SerializedProperty fromCam;

	public override SerializedProperty LastProperty => fromCam;

	protected virtual void OnEnable()
	{
		item = serializedObject.FindProperty("item");
		npcMagic = serializedObject.FindProperty("NPC_MAGIC");
		weapon = serializedObject.FindProperty("WEPPY");
		power = serializedObject.FindProperty("power");
		forward = serializedObject.FindProperty("FORWARD");
		Override = serializedObject.FindProperty("OVERRIDE");
		fromCam = serializedObject.FindProperty("FromCam");
	}

	public override void DrawGUI()
	{
		UnityEngine.GUI.enabled = Override.objectReferenceValue == null;
		
		EditorTools.DrawHelpProperty(item, "The resource asset to spawn.");
		
		UnityEngine.GUI.enabled = true;

		EditorGUI.BeginChangeCheck();

		EditorTools.DrawHelpProperty(Override, "The object to spawn. Overrides Item.");

		if (EditorGUI.EndChangeCheck())
		{
			if (Override.objectReferenceValue == null)
				item.stringValue = "";
			else
				item.stringValue = Override.objectReferenceValue.name;
		}

		EditorTools.DrawHelpProperty(npcMagic, "Treats the object spawned as magic, directly setting the power value on the damage trigger component.");

		UnityEngine.GUI.enabled = true;
		
		EditorTools.DrawHelpProperty(weapon, "Sets the power of the damage trigger component based on calculations using this weapon, if NPC_MAGIC is false.");
		
		UnityEngine.GUI.enabled = npcMagic.boolValue || weapon.objectReferenceValue != null;

		EditorTools.DrawHelpProperty(power, "The base value to set the damage trigger power to.");

		UnityEngine.GUI.enabled = true;

		EditorTools.DrawHelpProperty(forward, "If the object should be spawned facing the same was as the player (active camera). Affected by weapon.");

		EditorTools.DrawHelpProperty(fromCam, "If the object should be fired from the player (active camera).");
	}
}
