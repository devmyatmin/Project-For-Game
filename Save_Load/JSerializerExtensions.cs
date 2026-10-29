using Beam.Serialization.Json;
using UnityEngine;

public static class JSerializerExtensions
{
	public static JObject SerializeGameObject(GameObject go)
	{
		JObject jObject = new JObject();
		jObject.AddField("active", JSerializer.Serialize(go.activeSelf));
		return jObject;
	}

	public static void DeserializeGameObject(JObject data, GameObject instance)
	{
		bool value = data.GetField("active").GetValue<bool>();
		instance.SetActive(value);
	}
}
