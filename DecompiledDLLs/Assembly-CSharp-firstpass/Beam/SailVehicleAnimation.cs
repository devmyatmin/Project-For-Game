using UnityEngine;

namespace Beam;

public class SailVehicleAnimation : VehicleAnimation<SailVehicleMovement>
{
	[SerializeField]
	private Animation _animation;

	protected override void Initialise()
	{
		PlaySailAnimation(raised: false);
		base.Initialise();
	}

	protected override void VehicleMovement_Operated(bool active)
	{
		base.VehicleMovement_Operated(active);
		PlaySailAnimation(active);
	}

	public void PlaySailAnimation(bool raised)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		foreach (AnimationState item in _animation)
		{
			item.time = ((!raised) ? 1 : 0);
			item.speed = (raised ? 1 : (-1));
		}
		_animation.Play();
	}

	protected override void VehicleMovement_Using()
	{
	}
}
