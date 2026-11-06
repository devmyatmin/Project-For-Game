using UnityEngine;

namespace Beam;

public class HelicopterVehicleAnimation : VehicleAnimation<HelicopterVehicleMovement>
{
	[SerializeField]
	private Light _warningLight;

	protected override void Start()
	{
		base.Start();
		if ((Object)(object)VehicleMovement != (Object)null)
		{
			VehicleMovement.EngineFailed += VehicleMovement_EngineFailed;
		}
	}

	protected override void OnDestroy()
	{
		if ((Object)(object)VehicleMovement != (Object)null)
		{
			VehicleMovement.EngineFailed -= VehicleMovement_EngineFailed;
		}
		base.OnDestroy();
	}

	protected override void VehicleMovement_Operated(bool active)
	{
		base.VehicleMovement_Operated(active);
		if (!active)
		{
			((Behaviour)_warningLight).enabled = false;
		}
		VehicleMovement.Operator.Character.SetInteger("Vehicle_Pose", active ? 2 : 0);
	}

	private void VehicleMovement_EngineFailed(bool failed)
	{
		((Behaviour)_warningLight).enabled = failed;
	}
}
