using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(Joint))]
public class JointSerializer : ComponentSerializer<Joint>
{
	public override JObject Save(Joint instance)
	{
		return new JObject((JObject joint) =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			joint.AddField("anchor", JSerializer.Serialize((object)instance.anchor));
			joint.AddField("autoConfigureConnectedAnchor", JSerializer.Serialize((object)instance.autoConfigureConnectedAnchor));
			joint.AddField("axis", JSerializer.Serialize((object)instance.axis));
			joint.AddField("breakForce", JSerializer.Serialize((object)instance.breakForce));
			joint.AddField("breakTorque", JSerializer.Serialize((object)instance.breakTorque));
			joint.AddField("enableCollision", JSerializer.Serialize((object)instance.enableCollision));
		});
	}

	public override void Load(JObject data, Joint instance)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		instance.anchor = JSerializer.Deserialize<Vector3>(data.GetField("anchor"));
		instance.autoConfigureConnectedAnchor = data.GetField("autoConfigureConnectedAnchor").GetValue<bool>();
		instance.axis = JSerializer.Deserialize<Vector3>(data.GetField("axis"));
		instance.breakForce = data.GetField("breakForce").GetValue<float>();
		instance.breakTorque = data.GetField("breakTorque").GetValue<float>();
		instance.enableCollision = data.GetField("enableCollision").GetValue<bool>();
	}
}
