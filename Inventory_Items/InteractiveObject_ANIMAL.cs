using Beam;
using Beam.Serialization.Json;

public class InteractiveObject_ANIMAL : InteractiveObject
{
	public override void Load(JObject data)
	{
		base.Load(data);
		if (TryGetComponent<LandAnimal>(out var component))
		{
			component.StartingPosition = base.transform.position;
		}
	}
}
