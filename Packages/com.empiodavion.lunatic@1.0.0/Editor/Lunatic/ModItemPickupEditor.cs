using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModItemPickup), true)]
public class ModItemPickupEditor : ModBaseEditor
{
	private SerializedProperty type;
	private SerializedProperty item;
	private SerializedProperty itemName;
	private SerializedProperty inChest;
	private SerializedProperty saved;

	private string[] materials;

	public override SerializedProperty LastProperty => item;

	private void OnEnable()
	{
		type = serializedObject.FindProperty("type");
		item = serializedObject.FindProperty("item");
		itemName = serializedObject.FindProperty("Name");
		inChest = serializedObject.FindProperty("inChest");
		saved = serializedObject.FindProperty("SAVED");
	}

	public override void DrawGUI()
	{
		Lunatic.Init();

		if (materials == null)
			materials = Lunatic.MaterialNames.ToArray();

		EditorGUI.BeginChangeCheck();

		EditorTools.DrawItemTypeProperty(type, "Item type contained in this pickup.");

		if (EditorGUI.EndChangeCheck())
		{
			itemName.stringValue = "";
			item.objectReferenceValue = null;
		}

		if (type.intValue == (int)Lunatic.ItemTypes.Material)
		{
			if (EditorTools.ShowHelp)
				EditorGUILayout.HelpBox("You can select one of Lunacid's materials from this list.", MessageType.Info);

			int index = EditorGUILayout.Popup("Select Lunacid material", -1, materials);

			if (index != -1)
				itemName.stringValue = Lunatic.MaterialNames[index];
		}

		if (type.intValue == (int)Lunatic.ItemTypes.Gold)
		{
			if (EditorTools.ShowHelp)
				EditorGUILayout.HelpBox("The amount of gold given by this pickup.", MessageType.Info);

			if (!int.TryParse(itemName.stringValue, out int amount))
				amount = 0;

			amount = EditorGUILayout.IntField("Amount", amount);

			itemName.stringValue = amount.ToString();
		}
		else
			DrawItem(item, itemName, "The object that is given when the pickup is collected.");

		EditorTools.DrawHelpProperty(inChest, "If this item pickup is contained in a chest.");
		EditorTools.DrawHelpProperty(saved, "The object that controls if this item pickup has been collected.");
	}

	private void DrawItem(SerializedProperty prop, SerializedProperty nameProp, string help)
	{
		if (EditorTools.ShowHelp)
			EditorGUILayout.HelpBox(help, MessageType.Info);

		EditorTools.DrawRefName(prop, nameProp);

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(prop);

		if (EditorGUI.EndChangeCheck())
		{
			if (prop.objectReferenceValue != null)
			{
				IModObject modObj;

				if (prop.objectReferenceValue is GameObject propGO)
					modObj = propGO.GetComponent<IModObject>();
				else
					modObj = prop.objectReferenceValue as IModObject;

				if (modObj is ModWeapon)
					type.intValue = (int)Lunatic.ItemTypes.Weapon;
				else if (modObj is ModMagic)
					type.intValue = (int)Lunatic.ItemTypes.Magic;
				else if (modObj is ModItem)
					type.intValue = (int)Lunatic.ItemTypes.Item;
				else if (modObj is ModMaterial)
					type.intValue = (int)Lunatic.ItemTypes.Material;
				else
				{
					Debug.LogWarning($"Could not find valid component that implements IModObject on {prop.objectReferenceValue.name}.");
					prop.objectReferenceValue = null;
				}

				EditorTools.CheckLunaticRef(prop, nameProp);
			}
			else
				nameProp.stringValue = "";
		}
	}
}
