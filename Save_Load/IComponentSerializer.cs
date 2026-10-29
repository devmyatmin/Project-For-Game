using Beam.Serialization.Json;
using UnityEngine;

public interface IComponentSerializer
{
	JObject Serialize(Component component);

	void Deserialize(JObject data, Component instance);
}
