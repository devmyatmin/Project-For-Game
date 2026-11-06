using Beam.Crafting;

namespace Beam;

public static class IPickupable_Extensions
{
	public static bool IsDummy(this IPickupable pickupable)
	{
		return pickupable is DummyPickupable;
	}
}
