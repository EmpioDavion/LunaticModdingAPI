using Newtonsoft.Json;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public abstract class LConditionBase : ScriptableObject, IModObject
{
	public Mod Mod { get; set; }
	public AssetBundle Bundle { get; set; }
	public string Name { get; set; }
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	[SerializeField, JsonProperty]
	protected string memberName;

	[SerializeField, JsonProperty]
	protected MemberTypes memberType;

	[JsonIgnore]
	public Object target;

	[JsonIgnore]
	public abstract System.Delegate Method { get; }

	public bool invert;

	protected System.Delegate GetDelegate(System.Type funcType)
	{
		if (target != null && !string.IsNullOrEmpty(memberName))
		{
			System.Type type = target.GetType();
			MethodInfo methodInfo = null;
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			if (memberType == MemberTypes.Property)
			{
				PropertyInfo propertyInfo = type.GetProperty(memberName, bindingFlags);
				methodInfo = propertyInfo.GetGetMethod();
			}
			else if (memberType == MemberTypes.Method)
				methodInfo = type.GetMethod(memberName, bindingFlags);

			if (methodInfo != null)
			{
				if (!MatchMethod(methodInfo))
				{
					string[] args = System.Array.ConvertAll(methodInfo.GetParameters(), (x) => x.ParameterType.Name);

					Debug.LogWarning($"Found method {methodInfo.ReturnType.Name} {methodInfo.Name}({string.Join(", ", args)})");
					
					args = System.Array.ConvertAll(funcType.GenericTypeArguments, (x) => x.Name);
					string retType = args.Length > 0 ? args[args.Length - 1] : "System.Void";

					Debug.LogWarning($"Expected type {retType}(${string.Join(", ", args, 0, args.Length - 1)})");
				}

				return System.Delegate.CreateDelegate(funcType, target, methodInfo);
			}
		}

		return null;
	}

	public abstract void Init();

	public abstract bool MatchMethod(MethodInfo _methodInfo);

	public static bool MatchMethod(LConditionBase condition, MethodInfo _methodInfo) => condition.MatchMethod(_methodInfo);
}

[System.Serializable]
public abstract class LConditionBase<T> : LConditionBase where T : System.Delegate
{
	[JsonIgnore]
	protected T method;

	[JsonIgnore]
	protected System.Func<bool> propertyMethod;

	[JsonIgnore]
	public override System.Delegate Method => method ?? (System.Delegate)propertyMethod;

	public override void Init()
	{
		if (memberType == MemberTypes.Property)
			propertyMethod = (System.Func<bool>)GetDelegate(typeof(System.Func<bool>));
		else
			method = (T)GetDelegate(typeof(T));
	}
}

[System.Serializable]
public class LCondition : LConditionBase<System.Func<bool>>
{
	public bool Invoke()
	{
		if (memberType == MemberTypes.Property)
			return propertyMethod == null || propertyMethod() != invert;

		return method == null || method() != invert;
	}

	public override bool MatchMethod(MethodInfo _methodInfo)
	{
		return _methodInfo.ReturnType == typeof(bool) &&
			_methodInfo.GetParameters().Length == 0;
	}
}

[System.Serializable]
public class LCondition<T> : LConditionBase<System.Func<T, bool>>
{
	[System.NonSerialized]
	private T argument;

	public bool Invoke(T _argument)
	{
		if (memberType == MemberTypes.Property)
			return propertyMethod == null || propertyMethod() != invert;

		argument = _argument;
		return method == null || method(argument) != invert;
	}

	public override bool MatchMethod(MethodInfo _methodInfo)
	{
		if (_methodInfo.ReturnType != typeof(bool))
			return false;

		ParameterInfo[] parameterInfos = _methodInfo.GetParameters();
		
		return parameterInfos.Length == 1 &&
			parameterInfos[0].ParameterType == typeof(T);
	}
}
