using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(CapsuleCollider))]
public class CapsuleColliderSerializer : ColliderSerializer
{
	public override JObject Save(Collider instance)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = base.Save(instance);
		jObject.AddField("center", JSerializer.Serialize((object)((CapsuleCollider)instance).center));
		jObject.AddField("radius", JSerializer.Serialize((object)((CapsuleCollider)instance).radius));
		jObject.AddField("height", JSerializer.Serialize((object)((CapsuleCollider)instance).height));
		jObject.AddField("direction", JSerializer.Serialize((object)((CapsuleCollider)instance).direction));
		return jObject;
	}

	public override void Load(JObject data, Collider instance)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data, instance);
		((CapsuleCollider)instance).center = JSerializer.Deserialize<Vector3>(data.GetField("center"));
		((CapsuleCollider)instance).radius = data.GetField("radius").GetValue<float>();
		((CapsuleCollider)instance).height = data.GetField("height").GetValue<float>();
		((CapsuleCollider)instance).direction = data.GetField("direction").GetValue<int>();
	}
}
