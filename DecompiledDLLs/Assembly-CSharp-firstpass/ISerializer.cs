using Beam.Serialization.Json;

public interface ISerializer
{
	JObject Serialize(object instance);

	object Deserialize(JObject data);
}
