using UnityEditor;

[CustomEditor(typeof(ModWeapon), true)]
public class ModWeaponEditor : ModBaseEditor
{
	private bool misc = true;
	private bool data = true;
	private bool idle = true;
	private bool attack = true;
	private bool block = true;
	private string[] elementTypes;

	private SerializedProperty glove;
    private SerializedProperty attackAnims;
    private SerializedProperty blockAnims;
    private SerializedProperty idleAnim;
    private SerializedProperty idleSpeed;
    private SerializedProperty readyAnim;
    private SerializedProperty swingSounds;
    private SerializedProperty blockSounds;
    private SerializedProperty soundPitches;
    private SerializedProperty description;
    private SerializedProperty collisionMask;
    private SerializedProperty swingCooldown;
    private SerializedProperty damage;
    private SerializedProperty reach;
    private SerializedProperty guard;
    private SerializedProperty weight;
    private SerializedProperty backstep;
    private SerializedProperty element;
    private SerializedProperty experience;
    private SerializedProperty growthRate;
    private SerializedProperty upgrade;
    private SerializedProperty upgradeWeapon;
	private SerializedProperty type;
	private SerializedProperty special;
    private SerializedProperty chargeSpeed;
    private SerializedProperty animationSpeed;
	private SerializedProperty idleAnimSet;
	private SerializedProperty attackAnimSet;
	private SerializedProperty swingSoundSet;
	private SerializedProperty blockAnimSet;
	private SerializedProperty blockSoundSet;

	public override SerializedProperty LastProperty => blockSoundSet;

	private void OnEnable()
	{
		elementTypes = typeof(Lunatic.ElementIcons).GetEnumNames();
		glove = serializedObject.FindProperty("Glove");
		attackAnims = serializedObject.FindProperty("Attack_Anims");
		blockAnims = serializedObject.FindProperty("Block_Anims");
		idleAnim = serializedObject.FindProperty("Idle");
		idleSpeed = serializedObject.FindProperty("Idle_Speed");
		readyAnim = serializedObject.FindProperty("Ready_anim");
		swingSounds = serializedObject.FindProperty("Swing_snds");
		blockSounds = serializedObject.FindProperty("Block_snds");
		soundPitches = serializedObject.FindProperty("snd_pitch");
		description = serializedObject.FindProperty("desc");
		collisionMask = serializedObject.FindProperty("Mask");
		swingCooldown = serializedObject.FindProperty("WEP_COOLDOWN");
		damage = serializedObject.FindProperty("WEP_DAMAGE");
		reach = serializedObject.FindProperty("WEP_REACH");
		guard = serializedObject.FindProperty("WEP_GUARD");
		weight = serializedObject.FindProperty("WEP_WEIGHT");
		backstep = serializedObject.FindProperty("WEP_BACKSTEP");
		element = serializedObject.FindProperty("WEP_ELEMENT");
		experience = serializedObject.FindProperty("WEP_XP");
		growthRate = serializedObject.FindProperty("WEP_GROWTH");
		upgrade = serializedObject.FindProperty("UPGRADE");
		upgradeWeapon = serializedObject.FindProperty("upgradeWeapon");
		type = serializedObject.FindProperty("type");
		special = serializedObject.FindProperty("special");
		chargeSpeed = serializedObject.FindProperty("CHARGE_SPEED");
		animationSpeed = serializedObject.FindProperty("anim_speed_adjust");
		idleAnimSet = serializedObject.FindProperty("idleAnimSet");
		attackAnimSet = serializedObject.FindProperty("attackAnimSet");
		swingSoundSet = serializedObject.FindProperty("swingSoundSet");
		blockAnimSet = serializedObject.FindProperty("blockAnimSet");
		blockSoundSet = serializedObject.FindProperty("blockSoundSet");
	}

	//[Tooltip("0 = normal, 1 fire, 2 ice, 3 poison, 4 light, 5 dark, 8 dark/light, 9 norm/fire, 10 ice/poi, 11 dark/fire")]
	//[Tooltip("-1 = no leveling, 0-99 is leveling, -2 is leveled up")]
	//[Tooltip("How much each attack XP goes up")]
	//[Tooltip("What new weapon does leveling this one give you")]
	//[Tooltip("0 melee, 1 ranged")]
	//[Tooltip("0 none, 1 fly in air, 2 Moonlight, 3 magic drain, 4 knock back, 5 moon sword with xp drain, 6 breakable, 7 hp drain, 8 crissagrim, 9 flail2, 10 flail3, 11 curse null, 13 cursed hammer, 14 switch type, 15 null curse/blind, 16 Death, 17 DeathisBack, 18 blind, 22 spawn shoot on full but no mana charge")]

	public override void DrawGUI()
	{
		EditorTools.DrawReloadLocalisation();
		EditorTools.DrawTranslation($"Weapons/{target.name}", false);
		EditorTools.DrawTranslation($"Weapon Descriptions/{target.name} details", true);

		if (misc = EditorGUILayout.Foldout(misc, "Misc"))
		{
			EditorGUI.indentLevel++;

			EditorTools.DrawHelpProperty(glove, "The root transform that the weapon mesh attaches to.");
			EditorTools.DrawHelpProperty(animationSpeed, "Speed multiplier for swing, attack and block animations.");

			if (soundPitches.arraySize != 2)
				soundPitches.arraySize = 2;

			EditorTools.DrawArrayElement(soundPitches, 0, "Minimum Pitch", "Minimum random pitch to use for weapon swing sounds.");
			EditorTools.DrawArrayElement(soundPitches, 1, "Maximum Pitch", "Maximum random pitch to use for weapon swing sounds.");

			EditorGUI.indentLevel--;
		}

		if (data = EditorGUILayout.Foldout(data, "Data"))
		{
			EditorGUI.indentLevel++;

			EditorTools.DrawHelpProperty(description, "Text description/lore of the weapon to show in the player's inventory.");
			EditorTools.DrawHelpProperty(weight, "How heavy the weapon is.");
			EditorTools.DrawHelpProperty(backstep, "The distance the weapon will make the player backstep.");

			EditorTools.DrawHelpProperty(growthRate, "How much experience to give per successful attack with the weapon, multiplied by elemental damage bonuses.");

			if ((int)experience.floatValue == -1 && !string.IsNullOrEmpty(upgrade.stringValue))
				EditorGUILayout.HelpBox("A weapon is set to upgrade to, but experience gain is disabled.", MessageType.Warning);
			else if ((int)experience.floatValue != -1 && string.IsNullOrEmpty(upgrade.stringValue))
				EditorGUILayout.HelpBox("No weapon set to upgrade to, but experience gain is enabled.", MessageType.Warning);
			
			EditorTools.DrawHelpProperty(experience, "The weapon's current experience amount, -1 disables leveling, 0-99 is leveling progress, -2 is leveled up.");

			DrawUpgrade(upgradeWeapon, upgrade, "The name of the new weapon given when this weapon is leveled up.");

			EditorTools.DrawHelpProperty(special, "Special effect of the weapon.");
			EditorTools.DrawHelpProperty(collisionMask, "Collision layers the weapon will detect collisions with for attacking and blocking.");

			EditorGUI.indentLevel--;
		}

		if (idle = EditorGUILayout.Foldout(idle, "Idle"))
		{
			EditorGUI.indentLevel++;

			EditorTools.DrawHelpProperty(idleSpeed, "Speed multiplier for the idle animation.");

			EditorTools.DrawHelpProperty(idleAnimSet, "Use Lunacid weapon idle animation set.");

			UnityEngine.GUI.enabled = idleAnimSet.intValue == 0;

			EditorGUI.indentLevel++;
			
			EditorTools.DrawHelpProperty(idleAnim, "Animation to use when the weapon is not swinging or blocking.");
			EditorTools.DrawHelpProperty(readyAnim, "The animation to play when the weapon is first pulled out on equip.");

			EditorGUI.indentLevel--;

			UnityEngine.GUI.enabled = true;

			EditorGUI.indentLevel--;
		}

		if (attack = EditorGUILayout.Foldout(attack, "Attack"))
		{
			EditorGUI.indentLevel++;

			EditorTools.DrawHelpProperty(type, "The attack style of the weapon.");
			EditorTools.DrawHelpProperty(damage, "How much damage the weapon inflicts on hit.");
			EditorTools.DrawHelpProperty(reach, "How far the weapon attack reaches.");
			if (EditorTools.ShowHelp)
				EditorGUILayout.HelpBox("The elemental type of the weapon's damage. Note that blood and random have no effect on melee weapons. On ranged weapons this does not affect the damage type of the projectile, only the type displayed in the menu. To change their element, change the element on the damage_trigger of the projectile.", MessageType.Info);
			element.intValue = EditorGUILayout.Popup(element.displayName, element.intValue, elementTypes);
			EditorTools.DrawHelpProperty(chargeSpeed, "How long the attack button must be held before fully charging the weapon attack.");
			EditorTools.DrawHelpProperty(swingCooldown, "Delay between each swing of the weapon.");

			EditorTools.DrawHelpProperty(attackAnimSet, "Use Lunacid weapon attack animation set.");

			UnityEngine.GUI.enabled = attackAnimSet.intValue == 0;

			EditorGUI.indentLevel++;

			EditorTools.DrawHelpProperty(attackAnims, "Animations to use for the attacking combo chain.");

			EditorGUI.indentLevel--;

			UnityEngine.GUI.enabled = true;

			EditorTools.DrawHelpProperty(swingSoundSet, "Use Lunacid weapon swing sound set.");

			UnityEngine.GUI.enabled = swingSoundSet.intValue == 0;

			EditorGUI.indentLevel++;

			EditorTools.DrawHelpProperty(swingSounds, "Sounds to randomly use for each attack animation.");

			EditorGUI.indentLevel--;

			UnityEngine.GUI.enabled = true;

			EditorGUI.indentLevel--;
		}

		if (block = EditorGUILayout.Foldout(block, "Block"))
		{
			EditorGUI.indentLevel++;

			EditorTools.DrawHelpProperty(guard, "Damage multiplier the player will receive from a hit when blocking.");

			if (blockAnims.arraySize != 4)
				blockAnims.arraySize = 4;

			EditorTools.DrawHelpProperty(blockAnimSet, "Use Lunacid weapon block animation set.");

			UnityEngine.GUI.enabled = blockAnimSet.intValue == 0;

			EditorGUI.indentLevel++;

			EditorTools.DrawArrayElement(blockAnims, 0, "Successful Block Anim", "Animation to play on a successful block.");
			EditorTools.DrawArrayElement(blockAnims, 1, "Mid Block Anim", "Animation to play when the player is hit before the weapon cooldown finishes from a previous block.");
			EditorTools.DrawArrayElement(blockAnims, 2, "Deflect Block Anim", "Animation to play when the player is hit when blocking.");
			EditorTools.DrawArrayElement(blockAnims, 3, "Guard Broken Anim", "Animation to play when the player's guard is broken.");

			EditorGUI.indentLevel--;

			UnityEngine.GUI.enabled = true;

			if (blockSounds.arraySize != 2)
				blockSounds.arraySize = 2;

			EditorTools.DrawHelpProperty(blockSoundSet, "Use Lunacid weapon block sound set.");

			UnityEngine.GUI.enabled = blockSoundSet.intValue == 0;

			EditorGUI.indentLevel++;

			EditorTools.DrawArrayElement(blockSounds, 0, "Block Deflect Sound", "Sound to play on successful blocking of an attack.");
			EditorTools.DrawArrayElement(blockSounds, 1, "Guard Broken Sound", "Sound to play when the player's guard is broken.");

			EditorGUI.indentLevel--;

			UnityEngine.GUI.enabled = true;

			EditorGUI.indentLevel--;
		}
	}

	private void DrawUpgrade(SerializedProperty prop, SerializedProperty nameProp, string help)
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
