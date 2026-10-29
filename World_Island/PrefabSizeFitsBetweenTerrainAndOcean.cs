namespace Beam;

public class PrefabSizeFitsBetweenTerrainAndOcean : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		float num = 0f;
		return ((Singleton<StrandedWorld>.Instance != null) ? Singleton<StrandedWorld>.Instance.GetHeight(placement.Position) : (-100f)) + settings.Size.y < num;
	}
}
