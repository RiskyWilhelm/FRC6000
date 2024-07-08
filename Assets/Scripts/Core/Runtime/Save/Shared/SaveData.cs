using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public sealed class SaveData
{
	[JsonProperty]
	public int chickenFoxExtinctionRate;


	// Initialize
	public SaveData()
	{ }
}