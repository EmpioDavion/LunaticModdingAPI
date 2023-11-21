using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters.Math;
using System.Collections.Generic;
using System.Reflection;

public static class LJson
{
	private static JsonSerializerSettings GlobalSettings;

	static LJson()
	{
		Assembly assembly = typeof(Vector3Converter).Assembly;
		System.Type[] types = assembly.GetTypes();
		System.Type converterType = typeof(JsonConverter);

		List<JsonConverter> converters = new List<JsonConverter>();

		foreach (System.Type type in types)
			if (!type.IsAbstract && type.IsSubclassOf(converterType))
				converters.Add((JsonConverter)System.Activator.CreateInstance(type));

		GlobalSettings = new JsonSerializerSettings() { Converters = converters };
	}

	public static string Serialise(object obj)
	{
		return JsonConvert.SerializeObject(obj, GlobalSettings);
	}

	public static T Deserialise<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json, GlobalSettings);
	}
}
