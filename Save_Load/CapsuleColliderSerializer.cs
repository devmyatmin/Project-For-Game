using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(CapsuleCollider))]
public class CapsuleColliderSerializer : ColliderSerializer
{
	public override JObject Save(Collider instance)
	{
		JObject jObject = base.Save(instance);
		jObject.AddField("center", JSerializer.Serialize(((CapsuleCollider)instance).center));
		jObject.AddField("radius", JSerializer.Serialize(((CapsuleCollider)instance).radius));
		jObject.AddField("height", JSerializer.Serialize(((CapsuleCollider)instance).height));
		jObject.AddField("direction", JSerializer.Serialize(((CapsuleCollider)instance).direction));
		return jObject;
	}

	public override void Load(JObject data, Collider instance)
	{
		base.Load(data, instance);
		((CapsuleCollider)instance).center = JSerializer.Deserialize<Vector3>(data.GetField("center"));
		((CapsuleCollider)instance).radius = data.GetField("radius").GetValue<float>();
		((CapsuleCollider)instance).height = data.GetField("height").GetValue<float>();
		((CapsuleCollider)instance).direction = data.GetField("direction").GetValue<int>();
	}
}
