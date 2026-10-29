using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Quaternion))]
public class QuaternionSerializer : Serializer<Quaternion>
{
	public override JObject Save(Quaternion instance)
	{
		return new JObject(delegate(JObject quaternion)
		{
			quaternion.AddField("x", instance.x);
			quaternion.AddField("y", instance.y);
			quaternion.AddField("z", instance.z);
			quaternion.AddField("w", instance.w);
		});
	}

	public override Quaternion Load(JObject data)
	{
		return new Quaternion(data["x"].GetValue<float>(), data["y"].GetValue<float>(), data["z"].GetValue<float>(), data["w"].GetValue<float>());
	}
}
