// Solutions to prevent serialization errors. Seen in https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/
// Newtonsoft struggles serializing structs like Vector3 because it has a property .normalized
// that references Vector3, and thus entering a self-reference loop throwing circular reference error.
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine;
using System;

public sealed class NewtonsoftQuaternionConverter : JsonConverter<Quaternion>
{
	public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
	{
		JObject obj = new JObject
		(
			new JProperty(nameof(Quaternion.x), value.x),
			new JProperty(nameof(Quaternion.y), value.y),
			new JProperty(nameof(Quaternion.z), value.z),
			new JProperty(nameof(Quaternion.w), value.w)
		);

		obj.WriteTo(writer);
	}

	public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject obj = JObject.Load(reader);
		return new Quaternion
		(
			obj.Value<float>(nameof(Quaternion.x)),
			obj.Value<float>(nameof(Quaternion.y)),
			obj.Value<float>(nameof(Quaternion.z)),
			obj.Value<float>(nameof(Quaternion.w))
		);
	}
}