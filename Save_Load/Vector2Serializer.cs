using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Vector2))]
public class Vector2Serializer : Serializer<Vector2>
{
	public override JObject Save(Vector2 instance)
	{
		return new JObject(delegate(JObject vector2)
		{
			vector2.AddField("x", instance.x);
			vector2.AddField("y", instance.y);
		});
	}

	public override Vector2 Load(JObject data)
	{
		return new Vector2(data.GetField("x").GetValue<float>(), data.GetField("y").GetValue<float>());
	}
}
