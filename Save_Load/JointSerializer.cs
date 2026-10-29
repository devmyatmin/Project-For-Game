using Beam.Serialization.Json;
using UnityEngine;

[ComponentSerializerFor(typeof(Joint))]
public class JointSerializer : ComponentSerializer<Joint>
{
	public override JObject Save(Joint instance)
	{
		return new JObject(delegate(JObject joint)
		{
			joint.AddField("anchor", JSerializer.Serialize(instance.anchor));
			joint.AddField("autoConfigureConnectedAnchor", JSerializer.Serialize(instance.autoConfigureConnectedAnchor));
			joint.AddField("axis", JSerializer.Serialize(instance.axis));
			joint.AddField("breakForce", JSerializer.Serialize(instance.breakForce));
			joint.AddField("breakTorque", JSerializer.Serialize(instance.breakTorque));
			joint.AddField("enableCollision", JSerializer.Serialize(instance.enableCollision));
		});
	}

	public override void Load(JObject data, Joint instance)
	{
		instance.anchor = JSerializer.Deserialize<Vector3>(data.GetField("anchor"));
		instance.autoConfigureConnectedAnchor = data.GetField("autoConfigureConnectedAnchor").GetValue<bool>();
		instance.axis = JSerializer.Deserialize<Vector3>(data.GetField("axis"));
		instance.breakForce = data.GetField("breakForce").GetValue<float>();
		instance.breakTorque = data.GetField("breakTorque").GetValue<float>();
		instance.enableCollision = data.GetField("enableCollision").GetValue<bool>();
	}
}
