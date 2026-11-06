using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Vector2))]
public class Vector2Serializer : Serializer<Vector2>
{
	public override JObject Save(Vector2 instance)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new JObject((JObject vector2) =>
		{
			vector2.AddField("x", instance.x);
			vector2.AddField("y", instance.y);
		});
	}

	public override Vector2 Load(JObject data)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(data.GetField("x").GetValue<float>(), data.GetField("y").GetValue<float>());
	}
}
