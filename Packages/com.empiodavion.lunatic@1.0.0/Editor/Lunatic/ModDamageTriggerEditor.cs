using UnityEditor;

[CustomEditor(typeof(ModDamageTrigger))]
public class ModDamageTriggerEditor : ModBaseEditor
{
	protected SerializedProperty power;
	protected SerializedProperty element;
	protected SerializedProperty shake;
	protected SerializedProperty shakeLength;
	protected SerializedProperty force;
	protected SerializedProperty impact;
	protected SerializedProperty physical;
	protected SerializedProperty effectPlayer;
	protected SerializedProperty onlyPL;
	protected SerializedProperty delay;
	protected SerializedProperty constant;
	protected SerializedProperty frequency;
	protected SerializedProperty childEffect;
	protected SerializedProperty arrow;
	protected SerializedProperty noStop;
	protected SerializedProperty trail;

	public override SerializedProperty LastProperty => trail;

	protected virtual void OnEnable()
	{
		power = serializedObject.FindProperty("power");
		element = serializedObject.FindProperty("element");
		shake = serializedObject.FindProperty("shake");
		shakeLength = serializedObject.FindProperty("shake_length");
		force = serializedObject.FindProperty("force");
		impact = serializedObject.FindProperty("IMP");
		physical = serializedObject.FindProperty("physical");
		effectPlayer = serializedObject.FindProperty("EffectPlayer");
		onlyPL = serializedObject.FindProperty("OnlyPL");
		delay = serializedObject.FindProperty("Delay");
		constant = serializedObject.FindProperty("Constant");
		frequency = serializedObject.FindProperty("frequency");
		childEffect = serializedObject.FindProperty("Child_Effect");
		arrow = serializedObject.FindProperty("arrow");
		noStop = serializedObject.FindProperty("no_stop");
		trail = serializedObject.FindProperty("Trail");
	}

	public override void DrawGUI()
	{
		EditorTools.DrawHelpProperty(power, "How much damage the trigger inflicts.");

		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("The damage element type.", MessageType.Info);

		Lunatic.Elements elements = (Lunatic.Elements)element.intValue;
		element.intValue = (int)(Lunatic.Elements)EditorGUILayout.EnumPopup(element.displayName, elements);

		EditorTools.DrawHelpProperty(shake, "The amount to shake the player's camera when they're hit.");
		EditorTools.DrawHelpProperty(shakeLength, "The duration of the player's camera shake.");
		EditorTools.DrawHelpProperty(force, "The strength of the knockback on hit.");
		EditorTools.DrawHelpProperty(impact, "If an impact particle should be created based on the tag of the object hit. Check Lunacid/Resources/impacts/");
		EditorTools.DrawHelpProperty(physical, "If the trigger should destroy whatever is hit by the trigger, removing it from the scene.");
		EditorTools.DrawHelpProperty(effectPlayer, "If the trigger should affect the player.");
		EditorTools.DrawHelpProperty(onlyPL, "If the trigger should NOT affect non-player objects.");
		EditorTools.DrawHelpProperty(delay, "Time in seconds before the trigger can affect objects when activated.");
		EditorTools.DrawHelpProperty(constant, "If the trigger continually applies damage to objects inside it.");

		UnityEngine.GUI.enabled = constant.boolValue;
		EditorTools.DrawHelpProperty(frequency, "The frequency objects inside the trigger are damaged.");
		UnityEngine.GUI.enabled = true;

		EditorTools.DrawHelpProperty(childEffect, "If the first child transform of this object should be activated on hitting an object.");

		UnityEngine.GUI.enabled = childEffect.boolValue;
		EditorTools.DrawHelpProperty(arrow, "If the first child transform of this object should attach to the hit object.");
		UnityEngine.GUI.enabled = true;

		EditorTools.DrawHelpProperty(noStop, "If the trigger should be able to damage ON-CONTACT more than once.");
		EditorTools.DrawHelpProperty(trail, "Transform that serves as the trigger object's trail effect.");
	}
}
