using Newtonsoft.Json;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public abstract class LConditionBase : ScriptableObject, IModObject
{
	protected static readonly BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	public Mod Mod { get; set; }
	public AssetBundle Bundle { get; set; }
	public string Name { get; set; }
	public string AssetName { get; set; }
	public string InternalName => Lunatic.GetInternalName(this);

	[SerializeField, JsonProperty]
	protected string memberName;

	[JsonIgnore]
	public Object target;

	public bool invert;

	protected virtual MethodInfo GetMethod()
	{
		System.Type type = target.GetType();

		return type.GetMethod(memberName, flags);
	}

	protected System.Delegate ConvertMethod(System.Type funcType, MethodInfo methodInfo)
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

	protected T ConvertMethod<T>(MethodInfo methodInfo) where T : System.Delegate
	{
		return (T)ConvertMethod(typeof(T), methodInfo);
	}

	protected System.Delegate GetDelegate(System.Type funcType)
	{
		if (target != null && !string.IsNullOrEmpty(memberName))
		{
			MethodInfo methodInfo = GetMethod();

			if (methodInfo != null)
			{
				return ConvertMethod(funcType, methodInfo);
			}
		}

		return null;
	}

	protected T GetDelegate<T>() where T : System.Delegate
	{
		return (T)GetDelegate(typeof(T));
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

	public override void Init()
	{
		method = (T)GetDelegate(typeof(T));
	}
}

[System.Serializable]
public class LCondition : LConditionBase<System.Func<bool>>
{
	public virtual bool Invoke()
	{
		return method == null || method() != invert;
	}

	public override bool MatchMethod(MethodInfo _methodInfo)
	{
		return _methodInfo.ReturnType == typeof(bool) &&
			_methodInfo.GetParameters().Length == 0;
	}
}

[System.Serializable]
public class LCondition<T> : LConditionBase<System.Func<T, bool>> where T : Object
{
	[JsonIgnore]
	protected System.Func<bool> emptyMethod;

	public override void Init()
	{
		MethodInfo methodInfo = GetMethod();
		ParameterInfo[] parameters = methodInfo.GetParameters();

		if (parameters.Length == 0)
		{
			emptyMethod = ConvertMethod<System.Func<bool>>(methodInfo);
			method = RunEmpty;
		}
		else
			base.Init();
	}

	public virtual bool Invoke(T _argument)
	{
		return method == null || method(_argument) != invert;
	}

	public override bool MatchMethod(MethodInfo _methodInfo)
	{
		if (_methodInfo.ReturnType != typeof(bool))
			return false;

		ParameterInfo[] parameterInfos = _methodInfo.GetParameters();

		return parameterInfos.Length == 0 || parameterInfos.Length == 1 &&
			parameterInfos[0].ParameterType == typeof(LSceneObject);
	}

	protected bool RunEmpty(T arg)
	{
		return emptyMethod == null ? !invert : emptyMethod();
	}
}
