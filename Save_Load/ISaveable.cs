using Beam.Serialization.Json;

public interface ISaveable
{
	JObject Save();

	void Load(JObject data);
}
