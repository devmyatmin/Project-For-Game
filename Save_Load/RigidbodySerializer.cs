using Beam;
using Beam.Serialization.Json;
using Funlabs;
using UnityEngine;

[ComponentSerializerFor(typeof(Rigidbody))]
public class RigidbodySerializer : ComponentSerializer<Rigidbody>
{
	public override JObject Save(Rigidbody instance)
	{
		return new JObject(delegate(JObject rigidbody)
		{
			rigidbody.AddField("isKinematic", JSerializer.Serialize(instance.isKinematic));
			rigidbody.AddField("useGravity", JSerializer.Serialize(instance.useGravity));
			rigidbody.AddField("position", JSerializer.Serialize(instance.position));
			rigidbody.AddField("rotation", JSerializer.Serialize(instance.rotation));
			rigidbody.AddField("velocity", JSerializer.Serialize(instance.velocity));
			rigidbody.AddField("angularVelocity", JSerializer.Serialize(instance.angularVelocity));
			rigidbody.AddField("mass", JSerializer.Serialize(instance.mass));
			rigidbody.AddField("drag", JSerializer.Serialize(instance.drag));
			rigidbody.AddField("angularDrag", JSerializer.Serialize(instance.angularDrag));
			rigidbody.AddField("freezeRotation", JSerializer.Serialize(instance.freezeRotation));
			rigidbody.AddField("constraints", JSerializer.Serialize(instance.constraints));
			rigidbody.AddField("interpolation", JSerializer.Serialize(instance.interpolation));
			rigidbody.AddField("detectCollisions", JSerializer.Serialize(instance.detectCollisions));
			rigidbody.AddField("collisionDetectionMode", JSerializer.Serialize(instance.collisionDetectionMode));
		});
	}

	public override void Load(JObject data, Rigidbody instance)
	{
		instance.isKinematic = Game.Mode.IsClient() | JSerializer.Deserialize<bool>(data.GetField("isKinematic"));
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
