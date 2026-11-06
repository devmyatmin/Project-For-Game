using UnityEngine;

namespace Beam.Crafting;

public class Construction_STEPS : Construction_FOUNDATION, IHaveAttribute
{
	public override bool OwnerPlacing(Transform ghost, BeamRay ray, float yRotation)
	{
		return base.OwnerPlacing(ghost, ray, yRotation);
	}

	public override void Place()
	{
		base.Place();
	}
}
