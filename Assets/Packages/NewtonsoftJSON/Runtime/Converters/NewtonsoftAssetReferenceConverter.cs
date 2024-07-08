// Solutions to prevent serialization errors. Seen in https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/
// Newtonsoft struggles serializing structs like Vector3 because it has a property .normalized
// that references Vector3, and thus entering a self-reference loop throwing circular reference error.
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using UnityEngine.AddressableAssets;

public sealed class NewtonsoftAssetReferenceConverter : JsonConverter<AssetReference>
{
	public override void WriteJson(JsonWriter writer, AssetReference value, JsonSerializer serializer)
	{
		JObject obj = new JObject
		(
			new JProperty(nameof(AssetReference.AssetGUID), value.AssetGUID)
		);

		obj.WriteTo(writer);
	}

	public override AssetReference ReadJson(JsonReader reader, Type objectType, AssetReference existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		return JObject.Load(reader).Value<AssetReference>();
	}
}