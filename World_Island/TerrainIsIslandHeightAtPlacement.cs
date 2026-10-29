namespace Beam;

public class TerrainIsIslandHeightAtPlacement : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		if (!(Singleton<StrandedWorld>.Instance != null))
		{
			return false;
		}
		return Singleton<StrandedWorld>.Instance.GetHeight(placement.Position) > -20f;
	}
}
