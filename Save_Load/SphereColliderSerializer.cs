using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(SphereCollider))]
public class SphereColliderSerializer : ColliderSerializer
{
	public override JObject Save(Collider instance)
	{
		JObject jObject = base.Save(instance);
		jObject.AddField("center", JSerializer.Serialize(((SphereCollider)instance).center));
		jObject.AddField("radius", JSerializer.Serialize(((SphereCollider)instance).radius));
		return jObject;
	}

	public override void Load(JObject data, Collider instance)
	{
		base.Load(data, instance);
		((SphereCollider)instance).center = JSerializer.Deserialize<Vector3>(data.GetField("center"));
		((SphereCollider)instance).radius = data.GetField("radius").GetValue<float>();
	}
}
