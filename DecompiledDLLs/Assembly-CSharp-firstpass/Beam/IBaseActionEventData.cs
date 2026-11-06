using Beam.Crafting;
using UnityEngine;

namespace Beam;

public interface IBaseActionEventData
{
	Vector3 Position { get; }

	Vector3 OtherPosition { get; }

	CraftingType OtherType { get; }

	int OtherOwnerID { get; }

	float Value { get; }
}
