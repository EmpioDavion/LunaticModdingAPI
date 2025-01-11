[System.Serializable]
[Newtonsoft.Json.JsonConverter(typeof(LJsonParamConverter))]
public abstract class LJsonParam
{
	public abstract System.Type GetInnerType();

	public abstract bool TrySetValue(object newValue);
}

[System.Serializable]
public abstract class LJsonParam<T> : LJsonParam
{
	public static readonly System.Type InnerType = typeof(T);

	public T value;

	public LJsonParam() { }

	public override string ToString()
	{
		return $"{value}";
	}

	public override System.Type GetInnerType() => InnerType;

	public override bool TrySetValue(object newValue)
	{
		if (newValue is T tVal)
		{
			value = tVal;
			return true;
		}

		return false;
	}

	public static implicit operator T(LJsonParam<T> obj)
	{
		return obj.value;
	}
}

public class LJsonParamBool : LJsonParam<bool> { }
public class LJsonParamInt : LJsonParam<int> { }
public class LJsonParamFloat : LJsonParam<float> { }
public class LJsonParamString : LJsonParam<string> { }
public class LJsonParamVector3 : LJsonParam<UnityEngine.Vector3> { }
public class LJsonParamColour : LJsonParam<UnityEngine.Color> { }
public class LJsonParamAssetSourceArray : LJsonParam<AssetSource[]> { }
public class LJsonParamJsonTerm : LJsonParam<JsonTerm> { }
