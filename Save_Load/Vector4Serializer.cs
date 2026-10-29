using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Vector4))]
public class Vector4Serializer : Serializer<Vector4>
{
	public override JObject Save(Vector4 instance)
	{
		return new JObject(delegate(JObject vector4)
		{
			vector4.AddField("x", instance.x);
			vector4.AddField("y", instance.y);
			vector4.AddField("z", instance.z);
			vector4.AddField("w", instance.w);
		});
	}

	public override Vector4 Load(JObject data)
	{
		return new Vector4(data["x"].GetValue<float>(), data["y"].GetValue<float>(), data["z"].GetValue<float>(), data["w"].GetValue<float>());
	}
}
