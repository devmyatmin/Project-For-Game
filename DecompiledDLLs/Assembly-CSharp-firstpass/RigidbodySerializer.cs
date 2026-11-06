using Beam;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

[ComponentSerializerFor(typeof(Rigidbody))]
public class RigidbodySerializer : ComponentSerializer<Rigidbody>
{
	public override JObject Save(Rigidbody instance)
	{
		return new JObject((JObject rigidbody) =>
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			rigidbody.AddField("isKinematic", JSerializer.Serialize((object)instance.isKinematic));
			rigidbody.AddField("useGravity", JSerializer.Serialize((object)instance.useGravity));
			rigidbody.AddField("position", JSerializer.Serialize((object)instance.position));
			rigidbody.AddField("rotation", JSerializer.Serialize((object)instance.rotation));
			rigidbody.AddField("velocity", JSerializer.Serialize((object)instance.velocity));
			rigidbody.AddField("angularVelocity", JSerializer.Serialize((object)instance.angularVelocity));
			rigidbody.AddField("mass", JSerializer.Serialize((object)instance.mass));
			rigidbody.AddField("drag", JSerializer.Serialize((object)instance.drag));
			rigidbody.AddField("angularDrag", JSerializer.Serialize((object)instance.angularDrag));
			rigidbody.AddField("freezeRotation", JSerializer.Serialize((object)instance.freezeRotation));
			rigidbody.AddField("constraints", JSerializer.Serialize((object)instance.constraints));
			rigidbody.AddField("interpolation", JSerializer.Serialize((object)instance.interpolation));
			rigidbody.AddField("detectCollisions", JSerializer.Serialize((object)instance.detectCollisions));
			rigidbody.AddField("collisionDetectionMode", JSerializer.Serialize((object)instance.collisionDetectionMode));
		});
	}

	public override void Load(JObject data, Rigidbody instance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		instance.isKinematic = MPExtensions.IsClient(Game.Mode) | JSerializer.Deserialize<bool>(data.GetField("isKinematic"));
		instance.useGravity = data.GetField("useGravity").GetValue<bool>();
		instance.position = JSerializer.Deserialize<Vector3>(data.GetField("position"));
		instance.rotation = JSerializer.Deserialize<Quaternion>(data.GetField("rotation"));
		if (!instance.isKinematic)
		{
			instance.velocity = JSerializer.Deserialize<Vector3>(data.GetField("velocity"));
			instance.angularVelocity = JSerializer.Deserialize<Vector3>(data.GetField("angularVelocity"));
		}
		instance.mass = data.GetField("mass").GetValue<float>();
		instance.drag = data.GetField("drag").GetValue<float>();
		instance.angularDrag = data.GetField("angularDrag").GetValue<float>();
		instance.freezeRotation = data.GetField("freezeRotation").GetValue<bool>();
		instance.detectCollisions = data.GetField("detectCollisions").GetValue<bool>();
		instance.constraints = JSerializer.Deserialize<RigidbodyConstraints>(data.GetField("constraints"));
		instance.collisionDetectionMode = JSerializer.Deserialize<CollisionDetectionMode>(data.GetField("collisionDetectionMode"));
		instance.interpolation = JSerializer.Deserialize<RigidbodyInterpolation>(data.GetField("interpolation"));
	}
}
