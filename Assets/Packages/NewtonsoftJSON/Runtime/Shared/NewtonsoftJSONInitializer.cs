using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

public static partial class NewtonsoftJSONInitializer
{
	// Initialize
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad()
	{
		ConfigureSettings();
	}

	private static void ConfigureSettings()
	{
		JsonConvert.DefaultSettings = () => new JsonSerializerSettings
		{
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.Auto,
			Converters = new JsonConverter[]
			{
				new NewtonsoftColorConverter(),
				new NewtonsoftVector2Converter(),
				new NewtonsoftVector3Converter(),
				new NewtonsoftVector3IntConverter(),
				new NewtonsoftVector4Converter(),
				new NewtonsoftQuaternionConverter(),
				new NewtonsoftAssetReferenceConverter(),
			}
		};
	}
}

#if UNITY_EDITOR

public static partial class NewtonsoftJSONInitializer
{
	[InitializeOnLoadMethod]
	private static void OnEditorInitializeOnLoad()
	{
		ConfigureSettings();
	}
}

#endif