using UnityEngine;

namespace Beam;

public class TerrainIsBelowPlacement : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		return placement.Position.y > (((Object)(object)Singleton<StrandedWorld>.Instance != (Object)null) ? Singleton<StrandedWorld>.Instance.GetHeight(placement.Position) : (-100f));
	}
}
