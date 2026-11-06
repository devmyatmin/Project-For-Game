using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(SphereCollider))]
public class SphereColliderSerializer : ColliderSerializer
{
	public override JObject Save(Collider instance)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = base.Save(instance);
		jObject.AddField("center", JSerializer.Serialize((object)((SphereCollider)instance).center));
		jObject.AddField("radius", JSerializer.Serialize((object)((SphereCollider)instance).radius));
		return jObject;
	}

	public override void Load(JObject data, Collider instance)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data, instance);
		((SphereCollider)instance).center = JSerializer.Deserialize<Vector3>(data.GetField("center"));
		((SphereCollider)instance).radius = data.GetField("radius").GetValue<float>();
	}
}
