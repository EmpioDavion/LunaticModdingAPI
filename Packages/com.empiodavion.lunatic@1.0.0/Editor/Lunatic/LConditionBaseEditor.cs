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

	public override SerializedProperty LastProperty => targetObj;

	protected List<MethodInfo> methods;
	protected string[] methodNames;

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

		methodIndex = -1;

		if (targetObj.objectReferenceValue != null)
		{
			methods = GetMethods(targetObj.objectReferenceValue, out methodNames);
			methodIndex = methods.FindIndex((x) => x.Name == memberName.stringValue);
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
				methodIndex = methods.FindIndex((x) => x.Name == memberName.stringValue);
			}

			EditorGUI.BeginChangeCheck();

			methodIndex = EditorGUILayout.Popup("Method", methodIndex, methodNames);

			if (EditorGUI.EndChangeCheck())
			{
				memberName.stringValue = methodNames[methodIndex];
				memberType.intValue = (int)MemberTypes.Method;
			}
		}

		EditorGUI.indentLevel--;
	}

	private List<MethodInfo> GetMethods(Object obj, out string[] methodNames)
	{
		List<MethodInfo> methods = new List<MethodInfo>();
		LConditionBase condition = (LConditionBase)target;
		MethodInfo[] allMethods = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		foreach (MethodInfo method in allMethods)
			if (condition.MatchMethod(method))
				methods.Add(method);

		methodNames = methods.ConvertAll((x) => x.Name).ToArray();

		return methods;
	}

}
