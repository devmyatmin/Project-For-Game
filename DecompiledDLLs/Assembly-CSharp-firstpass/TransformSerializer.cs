using Beam;
using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(Transform))]
public class TransformSerializer : ComponentSerializer<Transform>
{
	public override JObject Save(Transform instance)
	{
		return new JObject((JObject transform) =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			transform.AddField("localPosition", JSerializer.Serialize((object)instance.localPosition));
			transform.AddField("localRotation", JSerializer.Serialize((object)instance.localRotation));
			transform.AddField("localScale", JSerializer.Serialize((object)Vector3Tools.Truncate(instance.localScale)));
		});
	}

	public override void Load(JObject data, Transform instance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		instance.localPosition = JSerializer.Deserialize<Vector3>(data.GetField("localPosition"));
		instance.localRotation = JSerializer.Deserialize<Quaternion>(data.GetField("localRotation"));
		instance.localScale = Vector3Tools.Truncate(JSerializer.Deserialize<Vector3>(data.GetField("localScale")));
	}
}
