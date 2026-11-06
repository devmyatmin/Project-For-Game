namespace Beam;

public class HeightIsInRange : PlacementValidator
{
	public override bool ValidatePlacement(Placement placement, IPlacementSettings settings)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		float num = settings.MinimumWorldHeight + settings.Size.y / 2f;
		float num2 = settings.MaximumWorldHeight - settings.Size.y / 2f;
		if (placement.Position.y > num)
		{
			return placement.Position.y < num2;
		}
		return false;
	}
}
