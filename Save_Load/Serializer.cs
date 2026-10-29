using Beam.Serialization.Json;

public abstract class Serializer<T> : ISerializer
{
	public JObject Serialize(object instance)
	{
		return Save((T)instance);
	}

	public object Deserialize(JObject data)
	{
		return Load(data);
	}

	public abstract JObject Save(T instance);

	public abstract T Load(JObject data);
}
