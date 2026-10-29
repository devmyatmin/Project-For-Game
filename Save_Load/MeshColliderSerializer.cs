using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(MeshCollider))]
public class MeshColliderSerializer : ColliderSerializer
{
	public override JObject Save(Collider instance)
	{
		JObject jObject = base.Save(instance);
		jObject.AddField("convex", JSerializer.Serialize(((MeshCollider)instance).convex));
		return jObject;
	}

	public override void Load(JObject data, Collider instance)
	{
		base.Load(data, instance);
		((MeshCollider)instance).convex = data.GetField("convex").GetValue<bool>();
	}
}
