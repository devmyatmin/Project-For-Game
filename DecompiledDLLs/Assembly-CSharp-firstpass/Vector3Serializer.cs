using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Vector3))]
public class Vector3Serializer : Serializer<Vector3>
{
	public override JObject Save(Vector3 instance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new JObject((JObject vector3) =>
		{
			vector3.AddField("x", instance.x);
			vector3.AddField("y", instance.y);
			vector3.AddField("z", instance.z);
		});
	}

	public override Vector3 Load(JObject data)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(data.GetField("x").GetValue<float>(), data.GetField("y").GetValue<float>(), data.GetField("z").GetValue<float>());
	}
}
