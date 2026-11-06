using UnityEngine;

namespace Beam;

public class PrefabSizeFitsBetweenTerrainAndOcean : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		return (((Object)(object)Singleton<StrandedWorld>.Instance != (Object)null) ? Singleton<StrandedWorld>.Instance.GetHeight(placement.Position) : (-100f)) + settings.Size.y < num;
	}
}
