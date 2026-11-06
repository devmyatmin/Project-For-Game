using Beam;
using Beam.Serialization.Json;
using UnityEngine;

public class InteractiveObject_ANIMAL : InteractiveObject
{
	public override void Load(JObject data)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.Load(data);
		LandAnimal landAnimal = default;
		if (((Component)this).TryGetComponent<LandAnimal>(ref landAnimal))
		{
			landAnimal.StartingPosition = transform.position;
		}
	}
}
