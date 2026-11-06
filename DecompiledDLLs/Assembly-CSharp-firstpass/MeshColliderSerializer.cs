using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(MeshCollider))]
public class MeshColliderSerializer : ColliderSerializer
{
	public override JObject Save(Collider instance)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = base.Save(instance);
		jObject.AddField("convex", JSerializer.Serialize((object)((MeshCollider)instance).convex));
		return jObject;
	}

	public override void Load(JObject data, Collider instance)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data, instance);
		((MeshCollider)instance).convex = data.GetField("convex").GetValue<bool>();
	}
}
