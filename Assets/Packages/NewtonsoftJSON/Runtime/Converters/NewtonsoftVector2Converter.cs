// Solutions to prevent serialization errors. Seen in https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/
// Newtonsoft struggles serializing structs like Vector3 because it has a property .normalized
// that references Vector3, and thus entering a self-reference loop throwing circular reference error.
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using System;

public sealed class NewtonsoftVector2Converter : JsonConverter<Vector2>
{
	public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
	{
		JObject obj = new JObject
		(
			new JProperty(nameof(Vector2.x), value.x),
			new JProperty(nameof(Vector2.y), value.y)
		);

		obj.WriteTo(writer);
	}

	public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject obj = JObject.Load(reader);
		return new Vector2
		(
			obj.Value<float>(nameof(Vector2.x)),
			obj.Value<float>(nameof(Vector2.y))
		);
	}
}

public sealed class NewtonsoftVector2IntConverter : JsonConverter<Vector2Int>
{
	public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
	{
		JObject obj = new JObject
		(
			new JProperty(nameof(Vector2Int.x), value.x),
			new JProperty(nameof(Vector2Int.y), value.y)
		);

		obj.WriteTo(writer);
	}

	public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject obj = JObject.Load(reader);
		return new Vector2Int
		(
			obj.Value<int>(nameof(Vector2Int.x)),
			obj.Value<int>(nameof(Vector2Int.y))
		);
	}
}