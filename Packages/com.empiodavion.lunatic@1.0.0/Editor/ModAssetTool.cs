using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ModAssetTool : EditorWindow
{
	private class TypeCategory
	{
		public string categoryName;

		public System.Type baseType;

		public List<System.Type> subTypes;

		public TypeCategory(string name, System.Type type)
		{
			categoryName = name;
			baseType = type;
			subTypes = new List<System.Type>();
		}

		public virtual void Create(System.Type type)
		{
			Object inst = CreateInstance(type);

			ProjectWindowUtil.CreateAsset(inst, $"{type.Name}.asset");
		}
	}

	private class PrefabTypeCategory : TypeCategory
	{
		private readonly string prefabPath;

		public PrefabTypeCategory(string name, System.Type type, string prefab) : base(name, type)
		{
			prefabPath = $"Packages/com.empiodavion.lunatic/Prefabs/{prefab}.prefab";
		}

		public override void Create(System.Type type)
		{
			GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			gameObject = Instantiate(gameObject);
			Component baseComponent = gameObject.GetComponent(baseType);
			Component replaceComponent = gameObject.AddComponent(type);

			EditorUtility.CopySerialized(baseComponent, replaceComponent);

			DestroyImmediate(baseComponent);

			string activeFolder = GetActiveFolder();
			string path = System.IO.Path.Combine(activeFolder, $"{type.Name}.prefab");
			int index = 1;

			while (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
				path = System.IO.Path.Combine(activeFolder, $"{type.Name} ({index++}).prefab");
			
			PrefabUtility.SaveAsPrefabAsset(gameObject, path);

			DestroyImmediate(gameObject);
		}
	}

	private readonly List<TypeCategory> typeCategories = new List<TypeCategory>
	{
		new TypeCategory("Games", typeof(ModGame)),
		new TypeCategory("Scenes", typeof(ModScene)),
		new PrefabTypeCategory("NPCs", typeof(ModMultipleStates), "BASE NPC"),
		new PrefabTypeCategory("Weapons", typeof(ModWeapon), "BASE WEAPON"),
		new PrefabTypeCategory("Magics", typeof(ModMagic), "BASE MAGIC"),
		new PrefabTypeCategory("Items", typeof(ModItem), "BASE ITEM"),
		new TypeCategory("Materials", typeof(ModMaterial)),
		new TypeCategory("Recipes", typeof(ModRecipe)),
		new TypeCategory("Classes", typeof(ModClass)),
	};

	private int category = 0;

	private Toolbar toolbar;
	private Box box;

	private static System.Func<string> GetActiveFolder;

	[MenuItem("Window/Mod Asset Tool")]
	private static void ShowWindow()
	{
		ModAssetTool wnd = GetWindow<ModAssetTool>();
		wnd.titleContent = new GUIContent("Mod Asset Tool");
		wnd.Show();
	}

	private void RebuildTypeLists()
	{
		foreach (TypeCategory typeCategory in typeCategories)
			typeCategory.subTypes.Clear();

		var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);

		foreach (var assembly in assemblies)
		{
			string path = System.IO.Path.Combine(Application.dataPath, "..", assembly.outputPath);

			System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFile(path);

			System.Type[] types = ass.GetTypes();

			AddTypes(types);
		}

		for (int i = 0; i < box.childCount; i++)
		{
			ListView listView = (ListView)box[i];
			listView.Refresh();
		}
	}

	private void AddTypes(System.Type[] types)
	{
		foreach (System.Type type in types)
		{
			foreach (TypeCategory typeCategory in typeCategories)
			{
				if (!type.IsAbstract && (type == typeCategory.baseType || type.IsSubclassOf(typeCategory.baseType)))
				{
					typeCategory.subTypes.Add(type);
					break;
				}
			}
		}
	}

	private void CreateGUI()
	{
		VisualElement root = rootVisualElement;

		root.Add(new Button(RebuildTypeLists) { text = "Refresh" });

		toolbar = new Toolbar();

		root.Add(toolbar);

		for (int i = 0; i < typeCategories.Count; i++)
		{
			int dummy = i;
			toolbar.Add(new ToolbarButton(() => ChangeCategory(dummy)) { text = typeCategories[i].categoryName });
		}

		box = new Box();
		box.style.flexGrow = 1.0f;
		
		root.Add(box);

		for (int i = 0; i < typeCategories.Count; i++)
		{
			ListView listView = new ListView();
			listView.visible = false;
			listView.style.position = Position.Absolute;
			listView.style.width = new StyleLength(new Length(100.0f, LengthUnit.Percent));
			listView.style.height = new StyleLength(new Length(100.0f, LengthUnit.Percent));
			listView.makeItem = () =>
			{
				Button button = new Button();
				button.style.maxWidth = 120;

				return button;
			};

			int dummy = i;
			listView.bindItem = (element, index) => BindItem(element, index, dummy);

			listView.itemHeight = 60;
			listView.itemsSource = typeCategories[i].subTypes;

			box.Add(listView);
		}

		box[0].visible = true;

		RebuildTypeLists();
		
		System.Type pwu = typeof(ProjectWindowUtil);
		var method = pwu.GetMethod("GetActiveFolderPath",
			System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

		System.Delegate del = System.Delegate.CreateDelegate(typeof(System.Func<string>), method);

		GetActiveFolder = (System.Func<string>)del;
	}

	private void BindItem(VisualElement element, int index, int i)
	{
		TypeCategory typeCategory = typeCategories[i];
		System.Type type = typeCategory.subTypes[index];

		Button button = (Button)element;
		button.text = type.Name;
		button.clicked += () => typeCategory.Create(type);
	}

	private void ChangeCategory(int newCategory)
	{
		box[category].visible = false;
		box[newCategory].visible = true;

		category = newCategory;

		box.MarkDirtyRepaint();
	}
}
