namespace Beam;

public class DirectX11PlacementValidator : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		if (SystemSpecs.CheckDirectX11Support())
		{
			return Options.GraphicsSettings.Preset.HigherThan(Quality_Preset.MEDIUM);
		}
		return false;
	}
}
