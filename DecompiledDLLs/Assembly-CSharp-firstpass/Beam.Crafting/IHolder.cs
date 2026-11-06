using System;

namespace Beam.Crafting;

public interface IHolder
{
	event Action<IPickupable> PickedUp;

	event Action<IPickupable> Dropped;
}
