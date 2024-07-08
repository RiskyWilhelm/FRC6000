// Solutions to prevent serialization errors. Seen in https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/
// Newtonsoft struggles serializing structs like Vector3 because it has a property .normalized
// that references Vector3, and thus entering a self-reference loop throwing circular reference error.
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using System;

public sealed class NewtonsoftVector4Converter : JsonConverter<Vector4>
{
	public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
	{
		JObject obj = new JObject
		(
			new JProperty(nameof(Vector4.x), value.x),
			new JProperty(nameof(Vector4.y), value.y),
			new JProperty(nameof(Vector4.z), value.z),
			new JProperty(nameof(Vector4.w), value.w)
		);

		obj.WriteTo(writer);
	}

	public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject obj = JObject.Load(reader);
		return new Vector4
		(
			obj.Value<float>(nameof(Vector4.x)),
			obj.Value<float>(nameof(Vector4.y)),
			obj.Value<float>(nameof(Vector4.z)),
			obj.Value<float>(nameof(Vector4.w))
		);
	}
}