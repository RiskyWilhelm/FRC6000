// Solutions to prevent serialization errors. Seen in https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/
// Newtonsoft struggles serializing structs like Vector3 because it has a property .normalized
// that references Vector3, and thus entering a self-reference loop throwing circular reference error.
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using System;

public sealed class NewtonsoftColorConverter : JsonConverter<Color>
{
	public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
	{
		JObject obj = new JObject
		(
			new JProperty(nameof(Color.r), value.r),
			new JProperty(nameof(Color.g), value.g),
			new JProperty(nameof(Color.b), value.b),
			new JProperty(nameof(Color.a), value.a)
		);

		obj.WriteTo(writer);
	}

	public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject obj = JObject.Load(reader);
		return new Color
		(
			obj.Value<float>(nameof(Color.r)),
			obj.Value<float>(nameof(Color.g)),
			obj.Value<float>(nameof(Color.b)),
			obj.Value<float>(nameof(Color.a))
		);
	}
}