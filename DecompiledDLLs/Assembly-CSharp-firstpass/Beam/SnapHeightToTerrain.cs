using UnityEngine;

namespace Beam;

public class SnapHeightToTerrain : PlacementModifier
{
	public override void ModifyPlacement(ref Placement placement, IPlacementSettings settings)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.Raycast(new Vector3(placement.Position.x, 10f, placement.Position.z), Vector3.down, ref val, 30f, (1 << Layers.TERRAIN) | (1 << Layers.TERRAIN_OBJECTS)))
		{
			placement.Position.y = val.point.y + settings.Size.y / 2f;
		}
	}
}
