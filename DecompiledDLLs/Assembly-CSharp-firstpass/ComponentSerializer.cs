using Beam.Serialization.Json;
using UnityEngine;

public abstract class ComponentSerializer<T> : IComponentSerializer where T : Component
{
	public JObject Serialize(Component component)
	{
		return Save((T)(object)component);
	}

	public void Deserialize(JObject data, Component instance)
	{
		Load(data, (T)(object)instance);
	}

	public abstract JObject Save(T instance);

	public abstract void Load(JObject data, T instance);
}
