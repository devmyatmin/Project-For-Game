using UnityEngine;

namespace Beam;

public interface IPlaceable
{
	float PlacingDistance { get; }

	void Place();

	bool OwnerPlacing(Transform ghost, BeamRay ray, float yRotation);

	bool ReplicantPlacing(Transform ghost, BeamRay ray, float yRotation);
}
