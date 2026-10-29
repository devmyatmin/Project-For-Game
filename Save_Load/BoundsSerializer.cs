using Beam.Serialization.Json;
using UnityEngine;

[CustomSerializerFor(typeof(Bounds))]
public class BoundsSerializer : Serializer<Bounds>
{
	public override JObject Save(Bounds instance)
	{
		return new JObject(delegate(JObject bounds)
		{
			bounds.AddField("center", JSerializer.Serialize(instance.center));
			bounds.AddField("extents", JSerializer.Serialize(instance.extents));
			bounds.AddField("min", JSerializer.Serialize(instance.min));
			bounds.AddField("max", JSerializer.Serialize(instance.max));
			bounds.AddField("size", JSerializer.Serialize(instance.size));
		});
	}

	public override Bounds Load(JObject data)
	{
		return default(Bounds);
	}
}
