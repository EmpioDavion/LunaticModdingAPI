using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LConditionBase), true)]
public class LConditionBaseEditor : ModBaseEditor
{
	protected SerializedProperty memberName;
	protected SerializedProperty memberType;
	protected SerializedProperty targetObj;
	protected SerializedProperty invert;

	public override SerializedProperty LastProperty => invert;

	protected List<MethodInfo> methods;
	protected string[] methodNames;

	protected List<Component> components;
	protected string[] componentNames;

	protected int methodIndex = -1;

	private void OnEnable()
	{
		if (target == null)
		{
			DestroyImmediate(this);
			return;
		}

		memberName = serializedObject.FindProperty("memberName");
		memberType = serializedObject.FindProperty("memberType");
		targetObj = serializedObject.FindProperty("target");
		invert = serializedObject.FindProperty("invert");

		methodIndex = -1;

		if (targetObj.objectReferenceValue != null)
		{
			methods = GetMethods(targetObj.objectReferenceValue, out methodNames);
			methodIndex = methods.FindIndex(MatchMethod);

			if (targetObj.objectReferenceValue is GameObject gameObject)
				components = GetComponents(gameObject, out componentNames);
			else if (targetObj.objectReferenceValue is Component component)
				components = GetComponents(component.gameObject, out componentNames);
			else
				components = null;
		}
	}

	public override void DrawGUI()
	{
		EditorGUILayout.LabelField(target.name);

		EditorGUI.indentLevel++;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PropertyField(targetObj);

		if (EditorGUI.EndChangeCheck())
		{
			methodIndex = -1;

			if (targetObj.objectReferenceValue != null)
				methods = GetMethods(targetObj.objectReferenceValue, out methodNames);
			else
			{
				methods = null;
				memberName.stringValue = "";
				memberType.intValue = 0;
			}
		}

		if (targetObj.objectReferenceValue != null)
		{
			if (methods == null)
			{
				methods = GetMethods(targetObj.objectReferenceValue, out methodNames);
				methodIndex = methods.FindIndex(MatchMethod);
			}

			if (components == null)
			{
				if (targetObj.objectReferenceValue is GameObject gameObject)
					components = GetComponents(gameObject, out componentNames);
				else if (targetObj.objectReferenceValue is Component component)
					components = GetComponents(component.gameObject, out componentNames);
			}

			if (components != null)
			{
				EditorGUI.BeginChangeCheck();

				int componentIndex = EditorGUILayout.Popup("Component", -1, componentNames);

				if (EditorGUI.EndChangeCheck())
				{
					targetObj.objectReferenceValue = components[componentIndex];
					methods = GetMethods(targetObj.objectReferenceValue, out methodNames);
					methodIndex = -1;

					Repaint();
				}
			}

			EditorGUI.BeginChangeCheck();

			methodIndex = EditorGUILayout.Popup("Method", methodIndex, methodNames);

			if (EditorGUI.EndChangeCheck())
			{
				memberName.stringValue = methodNames[methodIndex];
				memberType.intValue = (int)methods[methodIndex].MemberType;
			}
		}

		EditorGUILayout.PropertyField(invert);

		EditorGUI.indentLevel--;
	}

	private bool MatchMethod(MethodInfo method)
	{
		return method.Name == memberName.stringValue && method.MemberType == (MemberTypes)memberType.intValue;
	}

	private List<MethodInfo> GetMethods(Object obj, out string[] methodNames)
	{
		List<MethodInfo> methods = new List<MethodInfo>();
		LConditionBase condition = (LConditionBase)target;
		MethodInfo[] allMethods = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		foreach (MethodInfo method in allMethods)
			if (condition.MatchMethod(method))
				methods.Add(method);

		PropertyInfo[] allProperties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		foreach (PropertyInfo property in allProperties)
		{
			if (property.PropertyType == typeof(bool))
			{
				MethodInfo getMethod = property.GetMethod;

				if (getMethod != null)
					methods.Add(getMethod);
			}
		}

		methodNames = methods.ConvertAll((x) => x.Name).ToArray();

		return methods;
	}


	private List<Component> GetComponents(GameObject gameObject, out string[] componentNames)
	{
		List<Component> components = new List<Component>();

		gameObject.GetComponents(components);

		componentNames = components.ConvertAll((x) => x.GetType().Name).ToArray();

		return components;
	}
}
