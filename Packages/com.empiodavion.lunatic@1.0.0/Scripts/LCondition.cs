using Newtonsoft.Json;
using System.Reflection;

[System.Serializable]
public abstract class LConditionBase : UnityEngine.ScriptableObject
{
	[UnityEngine.SerializeField, JsonProperty]
	protected string memberName;

	[UnityEngine.SerializeField, JsonProperty]
	protected MemberTypes memberType;

	[JsonIgnore]
	public UnityEngine.Object target;

	[JsonIgnore]
	public abstract System.Delegate Method { get; }

	protected System.Delegate GetDelegate(System.Type funcType)
	{
		if (target != null)
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
				return System.Delegate.CreateDelegate(funcType, methodInfo);
		}

		return null;
	}

	public abstract void Init(UnityEngine.AssetBundle bundle);

	protected abstract bool InvokeFunc();

	public abstract bool MatchMethod(MethodInfo _methodInfo);

	public static bool MatchMethod(LConditionBase condition, MethodInfo _methodInfo) => condition.MatchMethod(_methodInfo);
}

[System.Serializable]
public abstract class LConditionBase<T> : LConditionBase where T : System.Delegate
{
	[JsonIgnore]
	protected T method;

	[JsonIgnore]
	public override System.Delegate Method => method;

	public override void Init(UnityEngine.AssetBundle bundle)
	{
		method = (T)GetDelegate(typeof(T));
		//target.Load(bundle);
	}
}

[System.Serializable]
public class LCondition : LConditionBase<System.Func<bool>>
{
	protected override bool InvokeFunc()
	{
		return method();
	}

	public bool Invoke()
	{
		return InvokeFunc();
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

	protected override bool InvokeFunc()
	{
		return method(argument);
	}

	public bool Invoke(T _argument)
	{
		argument = _argument;
		return InvokeFunc();
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
