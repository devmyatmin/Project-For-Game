using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Bounds))]
public class BoundsSerializer : Serializer<Bounds>
{
	public override JObject Save(Bounds instance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new JObject((JObject bounds) =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			bounds.AddField("center", JSerializer.Serialize((object)instance.center));
			bounds.AddField("extents", JSerializer.Serialize((object)instance.extents));
			bounds.AddField("min", JSerializer.Serialize((object)instance.min));
			bounds.AddField("max", JSerializer.Serialize((object)instance.max));
			bounds.AddField("size", JSerializer.Serialize((object)instance.size));
		});
	}

	public override Bounds Load(JObject data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return default;
	}
}
