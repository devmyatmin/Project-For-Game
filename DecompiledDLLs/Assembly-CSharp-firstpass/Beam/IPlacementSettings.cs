using UnityEngine;

namespace Beam;

public interface IPlacementSettings
{
	Vector3 Size { get; }

	float MinimumWorldHeight { get; }

	float MaximumWorldHeight { get; }
}
