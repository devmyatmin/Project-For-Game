using UnityEngine;

namespace Beam;

public class TerrainIsMinimumHeightAtPlacement : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return (((Object)(object)Singleton<StrandedWorld>.Instance != (Object)null) ? Singleton<StrandedWorld>.Instance.GetHeight(placement.Position) : (-100f)) > settings.MinimumWorldHeight;
	}
}
