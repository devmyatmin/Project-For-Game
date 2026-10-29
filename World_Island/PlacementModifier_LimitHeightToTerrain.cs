using UnityEngine;

namespace Beam.Crafting;

public class PlacementModifier_LimitHeightToTerrain : PlacementModifier
{
	public override Placement Modify(Placement placement, BeamRay ray)
	{
		if (Physics.Raycast(placement.Position + Vector3.up * 128f, Vector3.down, out var hitInfo, float.PositiveInfinity, 1 << Layers.TERRAIN))
		{
			Vector3 position = placement.Position;
			position.y = hitInfo.point.y + 0.1f;
			placement.Position = position;
		}
		return placement;
	}
}
