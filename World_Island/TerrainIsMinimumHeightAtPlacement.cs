namespace Beam;

public class TerrainIsMinimumHeightAtPlacement : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		return ((Singleton<StrandedWorld>.Instance != null) ? Singleton<StrandedWorld>.Instance.GetHeight(placement.Position) : (-100f)) > settings.MinimumWorldHeight;
	}
}
