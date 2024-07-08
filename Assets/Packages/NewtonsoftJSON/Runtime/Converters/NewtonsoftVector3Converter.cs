// Solutions to prevent serialization errors. Seen in https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/
// Newtonsoft struggles serializing structs like Vector3 because it has a property .normalized
// that references Vector3, and thus entering a self-reference loop throwing circular reference error.
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using System;

public sealed class NewtonsoftVector3Converter : JsonConverter<Vector3>
{
	public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
	{
		JObject obj = new JObject
		(
			new JProperty(nameof(Vector3.x), value.x),
			new JProperty(nameof(Vector3.y), value.y),
			new JProperty(nameof(Vector3.z), value.z)
		);

		obj.WriteTo(writer);
	}

	public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject obj = JObject.Load(reader);
		return new Vector3
		(
			obj.Value<float>(nameof(Vector3.x)),
			obj.Value<float>(nameof(Vector3.y)),
			obj.Value<float>(nameof(Vector3.z))
		);
	}
}

public sealed class NewtonsoftVector3IntConverter : JsonConverter<Vector3Int>
{
	public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
	{
		JObject obj = new JObject
		(
			new JProperty(nameof(Vector3Int.x), value.x),
			new JProperty(nameof(Vector3Int.y), value.y),
			new JProperty(nameof(Vector3Int.z), value.z)
		);

		obj.WriteTo(writer);
	}

	public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject obj = JObject.Load(reader);
		return new Vector3Int
		(
			obj.Value<int>(nameof(Vector3Int.x)),
			obj.Value<int>(nameof(Vector3Int.y)),
			obj.Value<int>(nameof(Vector3Int.z))
		);
	}
}