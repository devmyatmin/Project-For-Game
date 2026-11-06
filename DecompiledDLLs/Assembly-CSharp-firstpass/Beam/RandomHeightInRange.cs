using UnityEngine;

namespace Beam;

public class RandomHeightInRange : PlacementModifier
{
	public override void ModifyPlacement(ref Placement placement, IPlacementSettings settings)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		float num = settings.MinimumWorldHeight + settings.Size.y / 2f;
		float num2 = settings.MaximumWorldHeight - settings.Size.y / 2f;
		float num3 = Random.value * Mathf.Abs(num - num2);
		placement.Position.y = Mathf.Min(num, num2) + num3;
	}
}
