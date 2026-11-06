using Beam.Utilities;
using UnityEngine;

namespace Beam.Crafting;

public class PlacementModifier_SnapToConstructions : PlacementModifier_SnapToLayer
{
	public override Placement Modify(Placement placement, BeamRay ray)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Ray baseRay = ray.BaseRay;
		float length = ray.Length;
		LayerMask layermask = Layermask;
		Construction_DOOR componentRaycast = BeamPhysics.GetComponentRaycast<Construction_DOOR>(baseRay, length, layermask.value, RaycastHitGetBody.Rigidbody);
		Snapped = (Object)(object)componentRaycast != (Object)null;
		SnappedObject = ((componentRaycast != null) ? ((Component)componentRaycast).gameObject : null);
		if (Snapped)
		{
			return placement;
		}
		return base.Modify(placement, ray);
	}
}
