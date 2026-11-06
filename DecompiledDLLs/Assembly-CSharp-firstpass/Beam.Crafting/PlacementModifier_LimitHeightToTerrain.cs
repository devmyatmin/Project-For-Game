using UnityEngine;

namespace Beam.Crafting;

public class PlacementModifier_LimitHeightToTerrain : PlacementModifier
{
	public override Placement Modify(Placement placement, BeamRay ray)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.Raycast(placement.Position + Vector3.up * 128f, Vector3.down, ref val, float.PositiveInfinity, 1 << Layers.TERRAIN))
		{
			Vector3 position = placement.Position;
			position.y = val.point.y + 0.1f;
			placement.Position = position;
		}
		return placement;
	}
}
