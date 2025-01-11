using Newtonsoft.Json;
using System;

internal class LJsonParamConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType) => objectType.IsSubclassOf(typeof(LJsonParam));

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		LJsonParam param = (LJsonParam)(existingValue ?? Activator.CreateInstance(objectType));

		object value = serializer.Deserialize(reader, param.GetInnerType());

		if (!param.TrySetValue(value))
			UnityEngine.Debug.LogWarning("Failed to deserialize " + objectType.Name);

		return param;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
