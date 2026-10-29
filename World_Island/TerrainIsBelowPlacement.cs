namespace Beam;

public class TerrainIsBelowPlacement : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		return placement.Position.y > ((Singleton<StrandedWorld>.Instance != null) ? Singleton<StrandedWorld>.Instance.GetHeight(placement.Position) : (-100f));
	}
}
