using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(Collider))]
public class ColliderSerializer : ComponentSerializer<Collider>
{
	public override JObject Save(Collider instance)
	{
		return new JObject((JObject collider) =>
		{
			collider.AddField("enabled", JSerializer.Serialize((object)instance.enabled));
			collider.AddField("isTrigger", JSerializer.Serialize((object)instance.isTrigger));
		});
	}

	public override void Load(JObject data, Collider instance)
	{
		instance.enabled = data.GetField("enabled").GetValue<bool>();
		instance.isTrigger = data.GetField("isTrigger").GetValue<bool>();
	}
}
