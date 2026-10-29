using Beam;
using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(Transform))]
public class TransformSerializer : ComponentSerializer<Transform>
{
	public override JObject Save(Transform instance)
	{
		return new JObject(delegate(JObject transform)
		{
			transform.AddField("localPosition", JSerializer.Serialize(instance.localPosition));
			transform.AddField("localRotation", JSerializer.Serialize(instance.localRotation));
			transform.AddField("localScale", JSerializer.Serialize(instance.localScale.Truncate()));
		});
	}

	public override void Load(JObject data, Transform instance)
	{
		instance.localPosition = JSerializer.Deserialize<Vector3>(data.GetField("localPosition"));
		instance.localRotation = JSerializer.Deserialize<Quaternion>(data.GetField("localRotation"));
		instance.localScale = JSerializer.Deserialize<Vector3>(data.GetField("localScale")).Truncate();
	}
}
