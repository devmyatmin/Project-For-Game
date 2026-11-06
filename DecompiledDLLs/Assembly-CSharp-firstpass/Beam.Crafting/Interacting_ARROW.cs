using Beam.Serialization.Json;

namespace Beam.Crafting;

public class Interacting_ARROW : ProjectileObject
{
	public override void Load(JObject data)
	{
		base.Load(data);
		LevelLoader.CurrentLoader.DeferredLoading.Add(() =>
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (!IsPickedUp)
			{
				_ = transform.position.y;
				_ = 0f;
			}
		});
	}
}
