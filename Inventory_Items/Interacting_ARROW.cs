using Beam.Serialization.Json;

namespace Beam.Crafting;

public class Interacting_ARROW : ProjectileObject
{
	public override void Load(JObject data)
	{
		base.Load(data);
		LevelLoader.CurrentLoader.DeferredLoading.Add(delegate
		{
			if (!base.IsPickedUp)
			{
				_ = base.transform.position.y;
				_ = 0f;
			}
		});
	}
}
