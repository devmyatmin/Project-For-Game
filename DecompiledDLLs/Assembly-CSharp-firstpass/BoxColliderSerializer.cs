using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(BoxCollider))]
public class BoxColliderSerializer : ColliderSerializer
{
	public override JObject Save(Collider instance)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = base.Save(instance);
		jObject.AddField("center", JSerializer.Serialize((object)((BoxCollider)instance).center));
		jObject.AddField("size", JSerializer.Serialize((object)((BoxCollider)instance).size));
		return jObject;
	}

	public override void Load(JObject data, Collider instance)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data, instance);
		((BoxCollider)instance).center = JSerializer.Deserialize<Vector3>(data.GetField("center"));
		((BoxCollider)instance).size = JSerializer.Deserialize<Vector3>(data.GetField("size"));
	}
}
