using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters.Math;
using System.Collections.Generic;
using System.Reflection;

public static class LJson
{
	private static readonly JsonSerializerSettings GlobalSettings;

	static LJson()
	{
		Assembly assembly = typeof(Vector3Converter).Assembly;
		System.Type[] types = assembly.GetTypes();
		System.Type converterType = typeof(JsonConverter);

		List<JsonConverter> converters = new List<JsonConverter>();

		foreach (System.Type type in types)
			if (!type.IsAbstract && type.IsSubclassOf(converterType))
				converters.Add((JsonConverter)System.Activator.CreateInstance(type));

		GlobalSettings = new JsonSerializerSettings()
		{
			ContractResolver = new FieldContractResolver(),
			Converters = converters,
			Formatting = Formatting.Indented
		};

		JsonConvert.DefaultSettings = () => GlobalSettings;
	}

	public static string Serialise(object obj)
	{
		return JsonConvert.SerializeObject(obj);
	}

	internal static JObject SerialiseObject(object obj)
	{
		return JObject.FromObject(obj);
	}

	public static T Deserialise<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json, GlobalSettings);
	}

	internal static T DeserializeObject<T>(JObject obj)
	{
		return obj.ToObject<T>();
	}

	public static void Populate<T>(string json, T obj)
	{
		JsonConvert.PopulateObject(json, obj);
	}

	private class FieldContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
		{
			PropertyInfo[] privProperties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
			PropertyInfo[] pubProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			FieldInfo[] privFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo[] pubFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			List<JsonProperty> props = new List<JsonProperty>();

			if (memberSerialization == MemberSerialization.OptIn)
			{
				FilterMembersOptIn(props, privProperties, memberSerialization);
				FilterMembersOptIn(props, pubProperties, memberSerialization);

				FilterMembersOptIn(props, privFields, memberSerialization);
				FilterMembersOptIn(props, pubFields, memberSerialization);
			}
			else
			{
				FilterMembersOptIn(props, privProperties, memberSerialization);
				FilterMembers(props, pubProperties, memberSerialization);

				FilterMembersOptIn(props, privFields, memberSerialization);
				FilterMembers(props, pubFields, memberSerialization);
			}

			props.ForEach(p => { p.Writable = true; p.Readable = true; });

			return props;
		}

		private void FilterMembers(List<JsonProperty> props, MemberInfo[] members, MemberSerialization memberSerialization)
		{
			foreach (MemberInfo member in members)
				if (!member.IsDefined(typeof(JsonIgnoreAttribute), true))
					props.Add(CreateProperty(member, memberSerialization));
		}

		private void FilterMembersOptIn(List<JsonProperty> props, MemberInfo[] members, MemberSerialization memberSerialization)
		{
			foreach (MemberInfo member in members)
				if (member.IsDefined(typeof(JsonPropertyAttribute), true) || member.IsDefined(typeof(JsonRequiredAttribute)))
					props.Add(CreateProperty(member, memberSerialization));
		}
	}
}
