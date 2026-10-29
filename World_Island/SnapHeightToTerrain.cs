using UnityEngine;

namespace Beam;

public class SnapHeightToTerrain : PlacementModifier
{
	public override void ModifyPlacement(ref Placement placement, IPlacementSettings settings)
	{
		if (Physics.Raycast(new Vector3(placement.Position.x, 10f, placement.Position.z), Vector3.down, out var hitInfo, 30f, (1 << Layers.TERRAIN) | (1 << Layers.TERRAIN_OBJECTS)))
		{
			placement.Position.y = hitInfo.point.y + settings.Size.y / 2f;
		}
	}
}
