using Funlabs;

namespace Beam;

public class CoopValidator : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (MPExtensions.IsLocalCoop(Game.Mode))
		{
			return Options.GraphicsSettings.Preset.HigherThan(Quality_Preset.HIGH);
		}
		return true;
	}
}
