using UnityEngine;

namespace Beam;

public class MotorVehicleAudio : VehicleAudio<VehicleMovementBase>
{
	private MotorVehicleMovement _motorVehicleMovement;

	protected override void Start()
	{
		_motorVehicleMovement = VehicleMovement as MotorVehicleMovement;
		if ((Object)(object)_motorVehicleMovement != (Object)null)
		{
			_motorVehicleMovement.FuelRanOut += MotorVehicledMovement_FuelRanOut;
		}
		base.Start();
	}

	protected override void OnDestroy()
	{
		if ((Object)(object)_motorVehicleMovement != (Object)null)
		{
			_motorVehicleMovement.FuelRanOut -= MotorVehicledMovement_FuelRanOut;
		}
		base.OnDestroy();
	}

	protected override void VehicleMovement_Operated(bool active)
	{
		if (!((Object)(object)_motorVehicleMovement == (Object)null) && !(_motorVehicleMovement.Fuel <= 0f))
		{
			base.VehicleMovement_Operated(active);
		}
	}

	protected virtual void MotorVehicledMovement_FuelRanOut()
	{
		StopVehicleAudio();
	}
}
