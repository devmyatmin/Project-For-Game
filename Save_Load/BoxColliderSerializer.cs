using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(BoxCollider))]
public class BoxColliderSerializer : ColliderSerializer
{
	public override JObject Save(Collider instance)
	{
		JObject jObject = base.Save(instance);
		jObject.AddField("center", JSerializer.Serialize(((BoxCollider)instance).center));
		jObject.AddField("size", JSerializer.Serialize(((BoxCollider)instance).size));
		return jObject;
	}

	public override void Load(JObject data, Collider instance)
	{
		base.Load(data, instance);
		((BoxCollider)instance).center = JSerializer.Deserialize<Vector3>(data.GetField("center"));
		((BoxCollider)instance).size = JSerializer.Deserialize<Vector3>(data.GetField("size"));
	}
}
