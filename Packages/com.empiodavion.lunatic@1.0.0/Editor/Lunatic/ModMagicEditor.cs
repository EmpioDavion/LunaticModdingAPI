using UnityEditor;

[CustomEditor(typeof(ModMagic), true)]
public class ModMagicEditor : ModBaseEditor
{
	private string[] magicTypes;
	private string[] elementTypes;

	public SerializedProperty projectile;
	public SerializedProperty projectileName;
	public SerializedProperty description;
	public SerializedProperty icon;
	public SerializedProperty type;
	public SerializedProperty damage;
	public SerializedProperty element;
	public SerializedProperty chargeTime;
	public SerializedProperty minChargeTime;
	public SerializedProperty lifeCost;
	public SerializedProperty manaCost;
	public SerializedProperty colour;
	public SerializedProperty fadeSpeed;
	public SerializedProperty isBlood;
	public SerializedProperty sounds;

	public override SerializedProperty LastProperty => projectile;

	private void OnEnable()
	{
		magicTypes = typeof(Lunatic.MagicTypes).GetEnumNames();
		elementTypes = typeof(Lunatic.Elements).GetEnumNames();

		projectile = serializedObject.FindProperty("projectile");
		projectileName = serializedObject.FindProperty("MAG_CHILD");
		description = serializedObject.FindProperty("desc");
		icon = serializedObject.FindProperty("ICON");
		colour = serializedObject.FindProperty("MAG_COLOR");
		type = serializedObject.FindProperty("MAG_TYPE");
		damage = serializedObject.FindProperty("MAG_DAMAGE");
		element = serializedObject.FindProperty("MAG_ELEM");
		chargeTime = serializedObject.FindProperty("MAG_CHARGE_TIME");
		minChargeTime = serializedObject.FindProperty("MIN_CHARGE_TIME");
		lifeCost = serializedObject.FindProperty("MAG_LIFE");
		manaCost = serializedObject.FindProperty("MAG_COST");
		fadeSpeed = serializedObject.FindProperty("MAG_FADE");
		isBlood = serializedObject.FindProperty("MAG_BL");
		sounds = serializedObject.FindProperty("snds");
	}

	public override void DrawGUI()
	{
		DrawProjectile(projectile, projectileName, "The object to spawn on cast.");

		EditorTools.DrawHelpProperty(description, "The description shown in the player's inventory.");
		EditorTools.DrawHelpProperty(icon, "The sprite shown in the player's inventory and in the active quick item hotbar.");
		EditorTools.DrawHelpProperty(colour, "The colour to tint the icon.");

		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("The category the magic belongs to.", MessageType.Info);

		type.intValue = EditorGUILayout.Popup(type.displayName, type.intValue, magicTypes);

		EditorTools.DrawHelpProperty(damage, "How much damage the magic projectile inflicts.");

		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox("The damage type element of the magic.", MessageType.Info);

		element.intValue = EditorGUILayout.Popup(element.displayName, element.intValue, elementTypes);

		EditorTools.DrawHelpProperty(chargeTime, "How long the magic takes to charge to cast.");
		EditorTools.DrawHelpProperty(minChargeTime, "The shortest the charge time can become after bonuses from player stats.");
		EditorTools.DrawHelpProperty(lifeCost, "The health cost to cast the magic (blood magic only).");
		EditorTools.DrawHelpProperty(manaCost, "The mana cost to cast the magic (non-blood magic only).");
		EditorTools.DrawHelpProperty(fadeSpeed, "The speed to fade the casting flash.");
		EditorTools.DrawHelpProperty(isBlood, "If the magic is considered a blood magic.");

		if (sounds.arraySize != 4)
			sounds.arraySize = 4;

		EditorTools.DrawArrayElement(sounds, 0, "Cast Sound", "The sound that plays when the magic is cast.");
		EditorTools.DrawArrayElement(sounds, 1, "Fail Sound", "The sound that plays when the player stops charging the magic before the required charge time.");
		EditorTools.DrawArrayElement(sounds, 2, "Charging Sound", "The sound that plays when the player starts charging to cast the magic.");
		EditorTools.DrawArrayElement(sounds, 3, "Charged Sound", "The sound that plays when the magic is fully charged and ready to cast.");
	}

	private void DrawProjectile(SerializedProperty prop, SerializedProperty nameProp, string help)
	{
		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox(help, MessageType.Info);

		EditorTools.DrawRefName(prop, nameProp);

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(prop);

		if (EditorGUI.EndChangeCheck())
			EditorTools.CheckLunaticRef(prop, nameProp);
	}
}
