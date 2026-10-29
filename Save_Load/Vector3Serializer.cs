using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Vector3))]
public class Vector3Serializer : Serializer<Vector3>
{
	public override JObject Save(Vector3 instance)
	{
		return new JObject(delegate(JObject vector3)
		{
			vector3.AddField("x", instance.x);
			vector3.AddField("y", instance.y);
			vector3.AddField("z", instance.z);
		});
	}

	public override Vector3 Load(JObject data)
	{
		return new Vector3(data.GetField("x").GetValue<float>(), data.GetField("y").GetValue<float>(), data.GetField("z").GetValue<float>());
	}
}
