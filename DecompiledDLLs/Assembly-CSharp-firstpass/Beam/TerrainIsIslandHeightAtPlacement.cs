using UnityEngine;

namespace Beam;

public class TerrainIsIslandHeightAtPlacement : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Singleton<StrandedWorld>.Instance != (Object)null))
		{
			return false;
		}
		return Singleton<StrandedWorld>.Instance.GetHeight(placement.Position) > -20f;
	}
}
