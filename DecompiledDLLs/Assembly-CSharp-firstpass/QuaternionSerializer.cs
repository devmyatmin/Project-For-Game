using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Quaternion))]
public class QuaternionSerializer : Serializer<Quaternion>
{
	public override JObject Save(Quaternion instance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new JObject((JObject quaternion) =>
		{
			quaternion.AddField("x", instance.x);
			quaternion.AddField("y", instance.y);
			quaternion.AddField("z", instance.z);
			quaternion.AddField("w", instance.w);
		});
	}

	public override Quaternion Load(JObject data)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		return new Quaternion(data["x"].GetValue<float>(), data["y"].GetValue<float>(), data["z"].GetValue<float>(), data["w"].GetValue<float>());
	}
}
