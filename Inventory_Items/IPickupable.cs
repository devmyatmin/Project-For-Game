using System;
using UnityEngine;

namespace Beam;

public interface IPickupable : IAnimatablePrimary, ICraftingDisplayableStorable, IDisplayableStorable, IStorable, IBase, IHasCraftingType, ISaveablePrefab, ISaveableReference, ISaveable, ICraftingMaterial
{
	bool CanPickUp { get; set; }

	bool IsPickedUp { get; set; }

	Vector3 LocalHoldingPosition { get; }

	Vector3 LocalHoldingRotation { get; }

	int OwnerId { get; }

	int PreviousOwnerId { get; }

	event Action<IPickupable> PickedUp;

	void SetOwner(IPlayer player);

	void Hold(bool holding);

	void SetLOD(int level);
}
